using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Midwolf.GamesFramework.Services.Migrations
{
    public partial class migratedbupdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventState",
                table: "Events",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "3d4602f7-6530-4483-9c17-a42cab591620");

            migrationBuilder.UpdateData(
                table: "Entries",
                keyColumn: "Id",
                keyValue: 300,
                column: "CreatedAt",
                value: new DateTime(2019, 3, 28, 13, 18, 34, 997, DateTimeKind.Utc).AddTicks(206));

            migrationBuilder.UpdateData(
                table: "Entries",
                keyColumn: "Id",
                keyValue: 301,
                column: "CreatedAt",
                value: new DateTime(2019, 3, 28, 13, 12, 34, 997, DateTimeKind.Utc).AddTicks(1070));

            migrationBuilder.UpdateData(
                table: "Entries",
                keyColumn: "Id",
                keyValue: 302,
                column: "CreatedAt",
                value: new DateTime(2019, 3, 28, 13, 14, 34, 997, DateTimeKind.Utc).AddTicks(1088));

            migrationBuilder.UpdateData(
                table: "Entries",
                keyColumn: "Id",
                keyValue: 303,
                column: "CreatedAt",
                value: new DateTime(2019, 3, 28, 13, 16, 34, 997, DateTimeKind.Utc).AddTicks(1095));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "EndDate", "RuleSet", "StartDate" },
                values: new object[] { new DateTime(2019, 3, 29, 13, 10, 34, 955, DateTimeKind.Utc).AddTicks(4840), "{\"Interval\":3600,\"NumberEntries\":10,\"NumberRefferals\":0}", new DateTime(2019, 3, 28, 13, 10, 34, 955, DateTimeKind.Utc).AddTicks(4198) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2019, 3, 31, 13, 10, 34, 995, DateTimeKind.Utc).AddTicks(1303), new DateTime(2019, 3, 30, 13, 10, 34, 995, DateTimeKind.Utc).AddTicks(1296) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2019, 4, 2, 13, 10, 34, 995, DateTimeKind.Utc).AddTicks(2012), new DateTime(2019, 4, 1, 13, 10, 34, 995, DateTimeKind.Utc).AddTicks(2012) });

            migrationBuilder.UpdateData(
                table: "Games",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "LastUpdated" },
                values: new object[] { new DateTime(2019, 3, 28, 13, 10, 34, 954, DateTimeKind.Utc).AddTicks(7428), new DateTime(2019, 3, 28, 13, 10, 34, 954, DateTimeKind.Utc).AddTicks(8842) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventState",
                table: "Events");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "d4a44dcc-95b4-4257-b261-a225ff7f502e");

            migrationBuilder.UpdateData(
                table: "Entries",
                keyColumn: "Id",
                keyValue: 300,
                column: "CreatedAt",
                value: new DateTime(2019, 3, 12, 10, 2, 18, 167, DateTimeKind.Utc).AddTicks(111));

            migrationBuilder.UpdateData(
                table: "Entries",
                keyColumn: "Id",
                keyValue: 301,
                column: "CreatedAt",
                value: new DateTime(2019, 3, 12, 9, 56, 18, 167, DateTimeKind.Utc).AddTicks(1038));

            migrationBuilder.UpdateData(
                table: "Entries",
                keyColumn: "Id",
                keyValue: 302,
                column: "CreatedAt",
                value: new DateTime(2019, 3, 12, 9, 58, 18, 167, DateTimeKind.Utc).AddTicks(1066));

            migrationBuilder.UpdateData(
                table: "Entries",
                keyColumn: "Id",
                keyValue: 303,
                column: "CreatedAt",
                value: new DateTime(2019, 3, 12, 10, 0, 18, 167, DateTimeKind.Utc).AddTicks(1080));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "EndDate", "RuleSet", "StartDate" },
                values: new object[] { new DateTime(2019, 3, 13, 9, 54, 18, 119, DateTimeKind.Utc).AddTicks(4828), "{\"Interval\":\"hour\",\"NumberEntries\":10,\"NumberRefferals\":0}", new DateTime(2019, 3, 12, 9, 54, 18, 119, DateTimeKind.Utc).AddTicks(4387) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2019, 3, 15, 9, 54, 18, 164, DateTimeKind.Utc).AddTicks(8747), new DateTime(2019, 3, 14, 9, 54, 18, 164, DateTimeKind.Utc).AddTicks(8737) });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2019, 3, 17, 9, 54, 18, 164, DateTimeKind.Utc).AddTicks(9555), new DateTime(2019, 3, 16, 9, 54, 18, 164, DateTimeKind.Utc).AddTicks(9555) });

            migrationBuilder.UpdateData(
                table: "Games",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "LastUpdated" },
                values: new object[] { new DateTime(2019, 3, 12, 9, 54, 18, 118, DateTimeKind.Utc).AddTicks(7851), new DateTime(2019, 3, 12, 9, 54, 18, 118, DateTimeKind.Utc).AddTicks(9194) });
        }
    }
}
