using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var groupPermissionIds = new
            {
                User = Guid.NewGuid(),
                Project = Guid.NewGuid(),
                Role = Guid.NewGuid(),
                Status = Guid.NewGuid(),
                Label = Guid.NewGuid(),
                Skill = Guid.NewGuid(),
                Applicant = Guid.NewGuid(),
                Position = Guid.NewGuid()
            };

            // Chèn dữ liệu vào bảng GroupPermission
            migrationBuilder.InsertData(
                table: "groupPermissions",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { groupPermissionIds.User, "USER" },
                    { groupPermissionIds.Project, "PROJECT" },
                    { groupPermissionIds.Role, "ROLE" },
                    { groupPermissionIds.Status, "STATUS" },
                    { groupPermissionIds.Label, "LABEL" },
                    { groupPermissionIds.Skill, "SKILL" },
                    { groupPermissionIds.Applicant, "APPLICANT" },
                    { groupPermissionIds.Position, "POSITION" }
                });

            // Chèn dữ liệu vào bảng Permission
            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "name", "groupPermissionId" },
                values: new object[,]
                {
                    // USER
                    { Guid.NewGuid(), "ADD_USER", groupPermissionIds.User },
                    { Guid.NewGuid(), "UPDATE_USER", groupPermissionIds.User },
                    { Guid.NewGuid(), "TOGGLE_USER", groupPermissionIds.User },
                    { Guid.NewGuid(), "GET_LIST_USER", groupPermissionIds.User },
                    { Guid.NewGuid(), "CHANGE_PASSWORD_USER", groupPermissionIds.User },
                    { Guid.NewGuid(), "GET_DETAIL_USER", groupPermissionIds.User },

                    // PROJECT
                    { Guid.NewGuid(), "ADD_PROJECT", groupPermissionIds.Project },
                    { Guid.NewGuid(), "TOGGLE_VISIBLE_PROJECT", groupPermissionIds.Project },
                    { Guid.NewGuid(), "DELETE_PROJECT", groupPermissionIds.Project },
                    { Guid.NewGuid(), "UPDATE_PROJECT", groupPermissionIds.Project },
                    { Guid.NewGuid(), "READ_ALL_PROJECTS", groupPermissionIds.Project },
                    { Guid.NewGuid(), "SETTING_DETAIL_ALL_PROJECTS", groupPermissionIds.Project },

                    // ROLE
                    { Guid.NewGuid(), "ADD_ROLE", groupPermissionIds.Role },
                    { Guid.NewGuid(), "DELETE_ROLE", groupPermissionIds.Role },
                    { Guid.NewGuid(), "READ_DETAIL_ROLE", groupPermissionIds.Role },
                    { Guid.NewGuid(), "READ_LIST_ROLE", groupPermissionIds.Role },
                    { Guid.NewGuid(), "UPSERT_ROLE", groupPermissionIds.Role },

                    // STATUS
                    { Guid.NewGuid(), "READ_DEFAULT_STATUS", groupPermissionIds.Status },
                    { Guid.NewGuid(), "UPDATE_DEFAULT_STATUS", groupPermissionIds.Status },
                    { Guid.NewGuid(), "ADD_DEFAULT_STATUS", groupPermissionIds.Status },
                    { Guid.NewGuid(), "DELETE_DEFAULT_STATUS", groupPermissionIds.Status },

                    // LABEL
                    { Guid.NewGuid(), "READ_DEFAULT_LABEL", groupPermissionIds.Label },
                    { Guid.NewGuid(), "DELETE_DEFAULT_LABEL", groupPermissionIds.Label },
                    { Guid.NewGuid(), "ADD_DEFAULT_LABEL", groupPermissionIds.Label },
                    { Guid.NewGuid(), "UPDATE_DEFAULT_LABEL", groupPermissionIds.Label },

                    // SKILL
                    { Guid.NewGuid(), "CREATE_SKILL", groupPermissionIds.Skill },
                    { Guid.NewGuid(), "UPDATE_SKILL", groupPermissionIds.Skill },
                    { Guid.NewGuid(), "DELETE_SKILL", groupPermissionIds.Skill },
                    { Guid.NewGuid(), "GET_SKILL", groupPermissionIds.Skill },
                    { Guid.NewGuid(), "GET_SKILL_USER", groupPermissionIds.Skill },
                    { Guid.NewGuid(), "SKILL_USER", groupPermissionIds.Skill },

                    // APPLICANT
                    { Guid.NewGuid(), "GET_APPLICANT", groupPermissionIds.Applicant },
                    { Guid.NewGuid(), "ADD_APPLICANT", groupPermissionIds.Applicant },
                    { Guid.NewGuid(), "UPDATE_APPLICANT", groupPermissionIds.Applicant },
                    { Guid.NewGuid(), "DELETE_APPLICANT", groupPermissionIds.Applicant },

                    // POSITION
                    { Guid.NewGuid(), "GET_POSITION", groupPermissionIds.Position },
                    { Guid.NewGuid(), "CREATE_POSITION", groupPermissionIds.Position },
                    { Guid.NewGuid(), "UPDATE_POSITION", groupPermissionIds.Position },
                    { Guid.NewGuid(), "DELETE_POSITION", groupPermissionIds.Position }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }

    }
}
