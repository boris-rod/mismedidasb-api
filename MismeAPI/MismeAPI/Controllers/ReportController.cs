﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.Service;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetReport()
        {
            //hardcoded now
            var userId = 298;
            await _reportService.GetNutritionalReportAsync(userId);
            return Ok();
        }

        /// <summary>
        /// Get feed report.
        /// </summary>
        [HttpPost("feed-report")]
        public async Task<IActionResult> GetFeedReport()
        {
            //hardcoded now
            var userId = 298;
            await _reportService.GetFeedReportAsync(userId);
            return Ok();
        }
    }
}