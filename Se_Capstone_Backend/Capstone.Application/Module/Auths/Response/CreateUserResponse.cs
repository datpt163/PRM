
using Capstone.Domain.Enums;


namespace Capstone.Application.Module.Auths.Response
{
    public class CreateUserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserStatus Status { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Gender? Gender { get; set; }
        public DateTime? Dob { get; set; }

        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? PositionName {  get; set; }
        public Guid? RoleId { get; set; }
        public string RoleName { get; set; }



        public CreateUserResponse(Guid? roleId, string roleName, UserStatus status, string email, Guid id, string userName, string fullName, string phone, string avatar, string address, Gender? gender,
            DateTime? dob, string? bankAccount, string? bankAccountName, DateTime createDate, DateTime? updateDate, DateTime? deleteDate)
        {
            RoleId = roleId;
            RoleName = roleName;
            Status = status;
            Email = email;
            Id = id;
            UserName = userName;
            FullName = fullName;
            Phone = phone;
            Avatar = avatar;
            Address = address;
            Gender = gender;
            Dob = dob;
            BankAccount = bankAccount;
            BankAccountName = bankAccountName;
            CreateDate = createDate;
            UpdateDate = updateDate;
            DeleteDate = deleteDate;
        }
    }
}
