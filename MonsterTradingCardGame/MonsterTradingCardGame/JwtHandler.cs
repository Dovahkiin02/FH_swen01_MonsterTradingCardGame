using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame {
    internal static class JwtHandler {
        private static SymmetricSecurityKey key = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());
        private static string issuer = Guid.NewGuid().ToString();
        public static string getJwt(Guid userId) {
            var header = new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            var payload = new JwtPayload {
                { "iss", issuer },
                { "exp", DateTimeOffset.Now.ToUnixTimeSeconds() }
            };

            // Create the token
            var token = new JwtSecurityToken(header, payload);

            // Write the token to a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static bool validateJwt(string token, out string errMsg) {
            TokenValidationParameters parameters = new () {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                IssuerSigningKey = key,
            };

            try {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, parameters, out SecurityToken validatedToken);
                Console.WriteLine(validatedToken.ToString());
                errMsg = "";
                return true;
            } catch (SecurityTokenExpiredException e) {
                Console.WriteLine("Token expired");
                errMsg = e.Message;
                return false;
            } catch(Exception e) {
                Console.WriteLine(e);
                errMsg = e.Message;
                return false;
            }
        }
    }
}
