using System.Collections.Generic;
using FinancialSummaryApi.V1.UseCase;
using FinancialSummaryApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinancialSummaryApi.V1.Controllers
{
    [Route("api/v1/healthcheck")]
    [ApiController]
    [Produces("application/json")]
    public class HealthCheckController : BaseController
    {
        [HttpGet]
        [Route("ping")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), 200)]
        public IActionResult HealthCheck()
        {
            var result = new Dictionary<string, bool> { { "success", true } };

            return Ok(result);
        }

        [HttpGet]
        [Route("error")]
        public void ThrowError()
        {
            ThrowOpsErrorUsecase.Execute();
        }
    }
}
