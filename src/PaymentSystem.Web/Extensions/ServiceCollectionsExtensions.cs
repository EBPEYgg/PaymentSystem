using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Services;
using PaymentSystem.Domain.Data;
using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Models;
using PaymentSystem.Domain.Options;

namespace PaymentSystem.Web.Extensions
{
    public static class ServiceCollectionsExtensions
    {
        public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Orders API",
                    Version = "v1"
                });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return builder;
        }

        public static WebApplicationBuilder AddData(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<OrdersDbContext>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("Orders")));

            return builder;
        }

        public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ICartsService, CartsService>();
            builder.Services.AddScoped<IOrdersService, OrdersService>();
            builder.Services.AddScoped<IMerchantsService, MerchantsService>();

            return builder;
        }

        public static WebApplicationBuilder AddBearerAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<IdentityUserEntity, IdentityRoleUserEntity>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<OrdersDbContext>()
                .AddUserManager<UserManager<IdentityUserEntity>>()
                .AddRoleManager<RoleManager<IdentityRoleUserEntity>>()
                .AddUserStore<UserStore<IdentityUserEntity, IdentityRoleUserEntity, OrdersDbContext, long>>()
                .AddRoleStore<RoleStore<IdentityRoleUserEntity, OrdersDbContext, long>>();
            builder.Services
                .AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                            builder.Configuration["Authentication:JwtTokenPrivateKey"]!)),
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Authentication:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Authentication:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole(RoleConstants.Admin));
                options.AddPolicy("Merchant", policy => policy.RequireRole(RoleConstants.Merchant));
                options.AddPolicy("User", policy => policy.RequireRole(RoleConstants.User));
            });
            builder.Services.AddTransient<IAuthService, AuthService>();

            return builder;
        }

        public static WebApplicationBuilder AddOptions(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Authentication"));

            return builder;
        }
    }
}