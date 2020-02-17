﻿using System;

namespace MismeAPI.Common.DTO.Response
{
    public class ConceptResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Codename { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool RequirePersonalData { get; set; }
    }
}