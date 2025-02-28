using Microsoft.AspNetCore.Mvc;
//using TokoMart.Constants;

namespace TokoMart.Controllers
{
    [ApiController]
    [Route("api/tableconstants")]
    public class TableConstantsController : ControllerBase
    {
        [HttpGet("tables")]
        public IActionResult GetTables()
        {
            // Mengambil semua kelas yang ada dalam TableConstants
            var nestedClasses = typeof(TableConstants).GetNestedTypes();

            // List untuk menyimpan objek yang akan dikirim sebagai "payload"
            var tableNames = nestedClasses.Select(nestedClass => new
            {
                table_name = nestedClass.Name.ToLower()
            }).ToList();

            return Ok(new { payload = tableNames });
        }

        [HttpGet("{tableName}/fields")]
        public IActionResult GetFields(string tableName)
        {
            var nestedClasses = typeof(TableConstants).GetNestedTypes();

            var fieldNames = new List<object>();

            foreach (var nestedClass in nestedClasses)
            {
                if (nestedClass.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    var fields = nestedClass.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    // Loop untuk menambahkan setiap field ke dalam fieldNames list
                    foreach (var field in fields)
                    {
                        if (field.Name != "TableName") // Menghindari menambahkan nama tabel
                        {
                            fieldNames.Add(new { field_name = field.Name.ToLower() });
                        }
                    }

                    return Ok(new { payload = fieldNames });
                }
            }

            return NotFound(new { payload = "Table not found" });
        }
    }
}
