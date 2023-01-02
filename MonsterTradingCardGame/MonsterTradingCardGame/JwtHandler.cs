using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace MonsterTradingCardGame {
    internal static class JwtHandler {
        private static SymmetricSecurityKey key = new SymmetricSecurityKey(Guid.Parse("26dade5a-0fe7-4908-bc79-5f936cca646f").ToByteArray());
        private static string issuer = Guid.Parse("26dade5a-0fe7-4908-bc79-5f936cca646f").ToString();
        public static string getJwt(Guid userId) {
            var header = new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            var payload = new JwtPayload {
                { "iss", issuer },
                { "exp", DateTimeOffset.UtcNow.AddYears(2).ToUnixTimeSeconds() },
                { "userId", userId }
            };

            // Create the token
            var token = new JwtSecurityToken(header, payload);

            // Write the token to a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static Guid? validateJwt(string? token) {
            if (token == null || token == "") {
                return null;
            }
            TokenValidationParameters parameters = new () {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                IssuerSigningKey = key,
                };
            

            try {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, parameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                //Console.WriteLine(validatedToken.ToString());
                return Guid.Parse(jwtToken.Claims.First(claim => claim.Type == "userId").Value);
            } catch (SecurityTokenExpiredException e) {
                Console.WriteLine("Token expired");
                return null;
            } catch(Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
