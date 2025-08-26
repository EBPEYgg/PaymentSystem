using Microsoft.AspNetCore.Mvc;
using PaymentSystem.Filters;

namespace PaymentSystem.Web.Controllers
{
    [ApiController]
    [TypeFilter<ApiExceptionFilter>]
    public class ApiBaseController : ControllerBase
    {

    }
}