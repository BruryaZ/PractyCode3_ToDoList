using System;
using System.Collections.Generic;

namespace TodoApi.Models
{
    public class MyTask
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool? IsComplete { get; set; }
    }
}