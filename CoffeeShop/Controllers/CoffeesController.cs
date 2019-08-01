using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CoffeeShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoffeesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CoffeesController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        //[HttpGet]
        //public ActionResult Get()
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = "SELECT Id, Title, BeanType FROM Coffee";
        //            SqlDataReader reader = cmd.ExecuteReader();
        //            List<Coffee> coffees = new List<Coffee>();

        //            while (reader.Read())
        //            {
        //                Coffee coffee = new Coffee
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    Title = reader.GetString(reader.GetOrdinal("Title")),
        //                    BeanType = reader.GetString(reader.GetOrdinal("BeanType"))
        //                };

        //                coffees.Add(coffee);
        //            }
        //            reader.Close();

        //            return Ok(coffees);
        //        }
        //    }
        //}

        [HttpGet("{id}", Name = "GetCoffee")]
        public ActionResult<Coffee> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Title, BeanType
                        FROM Coffee
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    // this has to be declared because you're expecting something to fill it - that way if the reader doesn't run because nothing comes back
                    // you can then handle the error with the if statement below the reader.
                    Coffee coffee = null;

                    if (reader.Read())
                    {
                        coffee = new Coffee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            BeanType = reader.GetString(reader.GetOrdinal("BeanType"))
                        };
                    }

                    if (coffee == null)
                    {
                        return NotFound();
                    }

                    reader.Close();

                    return Ok(coffee);
                }
            }
        }

        [HttpGet]
        public ActionResult<List<Coffee>> Get([FromQuery] string beanType, [FromQuery] string sortBy)
        {
            if (beanType == null)
            {
                beanType = "";
            }

            if (sortBy == null || sortBy.ToLower() != "beantype")
            {
                sortBy = "BeanType";
            }
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Title, BeanType
                        FROM Coffee
                        WHERE BeanType LIKE '%' + @beanType + '%'";
                    cmd.Parameters.Add(new SqlParameter("@beanType", beanType));
                    cmd.Parameters.Add(new SqlParameter("@sorted", sortBy));
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Coffee> coffees = new List<Coffee>();

                    // this has to be declared because you're expecting something to fill it - that way if the reader doesn't run because nothing comes back
                    // you can then handle the error with the if statement below the reader.

                    while (reader.Read())
                    {
                        Coffee coffee = new Coffee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            BeanType = reader.GetString(reader.GetOrdinal("BeanType"))
                        };

                        coffees.Add(coffee);
                    }
                    reader.Close();

                    return Ok(coffees);
                }
            }
        }

        [HttpPost]
        public ActionResult<Coffee> Post([FromBody] Coffee coffee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Coffee (Title, BeanType)
                                        OUTPUT INSERTED.Id
                                        VALUES (@title, @beanType)";
                    cmd.Parameters.Add(new SqlParameter("@title", coffee.Title));
                    cmd.Parameters.Add(new SqlParameter("@beanType", coffee.BeanType));

                    int newId = (int)cmd.ExecuteScalar();
                    coffee.Id = newId;
                    return CreatedAtRoute("GetCoffee", new { id = newId }, coffee);

                    // or return this get:
                    // return RedirectToAction();
                }
            }
        }

        [HttpPut("{id}")]
        public ActionResult Put([FromRoute] int id, [FromBody] Coffee coffee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Coffee
                                            SET Title = @title,
                                            BeanType = @beanType
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@title", coffee.Title));
                        cmd.Parameters.Add(new SqlParameter("@beanType", coffee.BeanType));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if(rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("no rows affected");
                    }
                }
            }
            catch
            {
                if (!CoffeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Coffee WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!CoffeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CoffeeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Title, BeanType
                        FROM Coffee
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}