using APIs.DBUtility;
using APIs.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;


namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class LoginController : ControllerBase
    {

        /// <summary>
        /// 使用ID和密码进行登录
        /// </summary>
        /// <param name="login">token</param>
        /// <returns>是否登录成功</returns>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Login(Login login)
        {
            DBHelper dBHelper = new DBHelper();
            try
            {
                string sqlQueryPW;
                //登录方式为ID+密码
                if (login.UserType != "ADMIN")
                {
                     sqlQueryPW = @"SELECT PASSWORD,IS_VALID FROM " + login.UserType + @" WHERE ID = " + login.ID;
                }
                else {
                     sqlQueryPW = @"SELECT PASSWORD FROM " + login.UserType + @" WHERE ID = " + login.ID;
                }
                DataTable table = dBHelper.ExecuteTable(sqlQueryPW);
                if (table.Rows.Count != 0)
                {
                    DataRow Row = table.Rows[0];
                    if (Row["PASSWORD"].ToString() == login.Password)
                    {
                        if (login.UserType != "ADMIN")
                        {
                            if (Row["IS_VALID"].ToString() == "1")
                            {
                                JWTPayload jwt = new JWTPayload();
                                jwt.UserID = login.ID;
                                switch (login.UserType)
                                {
                                    case "ADMIN":
                                        jwt.UserType = 0;
                                        break;
                                    case "CUSTOMER":
                                        jwt.UserType = 1;
                                        break;
                                    case "SELLER":
                                        jwt.UserType = 2;
                                        break;
                                    default:
                                        break;
                                }
                                return Ok(JWTHelper.SetJwtEncode(jwt));
                            }
                            else
                                return BadRequest("该账号已被封禁！");
                        }
                        else {
                            JWTPayload jwt = new JWTPayload();
                            jwt.UserID = login.ID;
                            jwt.UserType = 0;
                            return Ok(JWTHelper.SetJwtEncode(jwt));
                        }
                    }
                    else
                        return BadRequest("账号与密码不符");
                }
                return BadRequest("账号不存在");
            }
            catch (OracleException oe)
            {
                return BadRequest("数据库请求出错"+oe.Number.ToString());
            }

        }
        

    }
}
