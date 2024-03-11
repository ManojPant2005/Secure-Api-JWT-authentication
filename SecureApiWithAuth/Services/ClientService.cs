using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureApiWithAuth.Data.Configuration;
using SecureApiWithAuth.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SecureApiWithAuth.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext appDbContext;
        private readonly IConfiguration config;
        public ClientService(AppDbContext appDbContext, IConfiguration config)
        {
            this.appDbContext = appDbContext;
            this.config = config;
        }
        public async Task<(bool flag, string Token)> LoginUserAsync(Login model)
        {
            var getUser = await appDbContext.Registrations.Where(_ => _.Email!.Equals(model.Email)).FirstOrDefaultAsync();
            if (getUser == null) return (false, string.Empty);

            var checkIfPasswordMatch = VerifyUserPassword(model.Password!, getUser.Password!);
            if (checkIfPasswordMatch)
            {
                //get user role from the roles table
                var getUserRole = await appDbContext.UserRoles.Where(_ => _.Id == getUser.Id).FirstOrDefaultAsync();

                //Generate token with the role, and username as email
                var token = GenerateToken(model.Email!, getUserRole!.RoleName!);

                var checkIfTokenExist = await appDbContext.TokenInfo.Where(_ => _.UserId == getUser.Id).FirstOrDefaultAsync();
                if (checkIfTokenExist == null)
                {
                    appDbContext.TokenInfo.Add(new TokenInfo() { Token = token, UserId = getUser.Id });
                    await appDbContext.SaveChangesAsync();
                    return (true, token);
                }
                checkIfTokenExist.Token = token;
                await appDbContext.SaveChangesAsync();
                return (true, token);
            }
            return (false, string.Empty);
        }

        //Decrypt user database password and encrypt user raw password and compare
        private static bool VerifyUserPassword(string rawPassword, string databasePassword)
        {
            byte[] dbPasswordHash = Convert.FromBase64String(databasePassword);
            byte[] salt = new byte[16];
            Array.Copy(dbPasswordHash, 0, salt, 0, 16);
            var rfcPassword = new Rfc2898DeriveBytes(rawPassword, salt, 1000, HashAlgorithmName.SHA1);
            byte[] rfcPasswordHash = rfcPassword.GetBytes(20);
            for (int i = 0; i < rfcPasswordHash.Length; i++)
            {
                if (dbPasswordHash[i + 16] != rfcPasswordHash[i])
                    return false;
            }
            return true;
        }

        private string GenerateToken(string email, string roleName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,email),
                new Claim(ClaimTypes.Role,roleName)
            };
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
        public async Task<(bool flag, string Message)> RegisterUserAsync(Registration model)
        {
            var userRole = new UserRole();
            //Check if admin already exist
            if (model.Email!.ToLower().StartsWith("admin"))
            {
                var chkIfAdminExist = await appDbContext.UserRoles.Where(_ => _.RoleName!.ToLower().Equals("admin")).FirstOrDefaultAsync();
                if (chkIfAdminExist != null) return (false, "Sorry Admin already exist, please change the email address");

                userRole.RoleName = "Admin";
            }

            var checkIfUserAlreadyCreated = await appDbContext.Registrations.Where(_ => _.Email!.ToLower().Equals(model.Email!.ToLower())).FirstOrDefaultAsync();
            if (checkIfUserAlreadyCreated != null) return (false, $"Email: {model.Email} already registered");


            var hashedPassword = HashPassword(model.Password!);
            model.Password = hashedPassword;
            var registeredEntity = appDbContext.Registrations.Add(model).Entity;
            await appDbContext.SaveChangesAsync();


            if (string.IsNullOrEmpty(userRole.RoleName))
                userRole.RoleName = "User";

            userRole.UserId = registeredEntity.Id;
            appDbContext.UserRoles.Add(userRole);
            await appDbContext.SaveChangesAsync();

            return (true, "Account Created");
        }

        // Encrypt user password
        private static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var randomGenerator = RandomNumberGenerator.Create())
            {
                randomGenerator.GetBytes(salt);
            }
            var rfcPassword = new Rfc2898DeriveBytes(password, salt, 1000, HashAlgorithmName.SHA1);
            byte[] rfcPasswordHash = rfcPassword.GetBytes(20);

            byte[] passwordHash = new byte[36];
            Array.Copy(salt, 0, passwordHash, 0, 16);
            Array.Copy(rfcPasswordHash, 0, passwordHash, 16, 20);
            return Convert.ToBase64String(passwordHash);
        }

    }
}
