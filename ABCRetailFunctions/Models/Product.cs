#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace ABCRetailFunctions.Models
{
    public class Product : ITableEntity
    {
        // Azure auto generated properties for Table Storage
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Product Properties
        public string? Name { get; set; }
        public string? Description { get; set; }

        //below rand symbol method is from chatgpt
        //            5.0 language model
        //           accessed on 28 aug 2025
        //           https://chatgpt.com/share/68b0d8c9-7e64-8002-bb17-add70d5dd3f6
        [DisplayFormat(DataFormatString = "R {0:N2}", ApplyFormatInEditMode = false)]
        public double? Price { get; set; }
        public int? StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }

        [IgnoreDataMember] // optional, if you don't want it serialized
        public string? ImageSasUrl { get; set; }

    }
}
