using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.Service;
using System;

namespace MismeAPI
{
    [Route("api/aas")]
    public class AaasController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;

        public AaasController(IReportService reportService, IMapper mapper)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get a report.
        /// </summary>
        [HttpPost]
        public IActionResult GetReport()
        {
            _reportService.GetNutritionalReport();
            return Ok();
        }
    }
}