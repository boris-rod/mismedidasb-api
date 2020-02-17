using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class LowerCaseTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answer_Question_QuestionId",
                table: "Answer");

            migrationBuilder.DropForeignKey(
                name: "FK_Poll_Concept_ConceptId",
                table: "Poll");

            migrationBuilder.DropForeignKey(
                name: "FK_Question_Poll_PollId",
                table: "Question");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswer_Answer_AnswerId",
                table: "UserAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswer_User_UserId",
                table: "UserAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPersonalData_PersonalData_PersonalDataId",
                table: "UserPersonalData");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPersonalData_User_UserId",
                table: "UserPersonalData");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPoll_Poll_PollId",
                table: "UserPoll");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPoll_User_UserId",
                table: "UserPoll");

            migrationBuilder.DropForeignKey(
                name: "FK_UserToken_User_UserId",
                table: "UserToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserToken",
                table: "UserToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPoll",
                table: "UserPoll");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPersonalData",
                table: "UserPersonalData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAnswer",
                table: "UserAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Question",
                table: "Question");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Poll",
                table: "Poll");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonalData",
                table: "PersonalData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Concept",
                table: "Concept");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Answer",
                table: "Answer");

            migrationBuilder.RenameTable(
                name: "UserToken",
                newName: "usertoken");

            migrationBuilder.RenameTable(
                name: "UserPoll",
                newName: "userpoll");

            migrationBuilder.RenameTable(
                name: "UserPersonalData",
                newName: "userpersonaldata");

            migrationBuilder.RenameTable(
                name: "UserAnswer",
                newName: "useranswer");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "user");

            migrationBuilder.RenameTable(
                name: "Question",
                newName: "question");

            migrationBuilder.RenameTable(
                name: "Poll",
                newName: "poll");

            migrationBuilder.RenameTable(
                name: "PersonalData",
                newName: "personaldata");

            migrationBuilder.RenameTable(
                name: "Concept",
                newName: "concept");

            migrationBuilder.RenameTable(
                name: "Answer",
                newName: "answer");

            migrationBuilder.RenameIndex(
                name: "IX_UserToken_UserId",
                table: "usertoken",
                newName: "IX_usertoken_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPoll_UserId",
                table: "userpoll",
                newName: "IX_userpoll_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPoll_PollId",
                table: "userpoll",
                newName: "IX_userpoll_PollId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPersonalData_UserId",
                table: "userpersonaldata",
                newName: "IX_userpersonaldata_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPersonalData_PersonalDataId",
                table: "userpersonaldata",
                newName: "IX_userpersonaldata_PersonalDataId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswer_UserId",
                table: "useranswer",
                newName: "IX_useranswer_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswer_AnswerId",
                table: "useranswer",
                newName: "IX_useranswer_AnswerId");

            migrationBuilder.RenameIndex(
                name: "IX_Question_PollId",
                table: "question",
                newName: "IX_question_PollId");

            migrationBuilder.RenameIndex(
                name: "IX_Poll_ConceptId",
                table: "poll",
                newName: "IX_poll_ConceptId");

            migrationBuilder.RenameIndex(
                name: "IX_Answer_QuestionId",
                table: "answer",
                newName: "IX_answer_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usertoken",
                table: "usertoken",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userpoll",
                table: "userpoll",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userpersonaldata",
                table: "userpersonaldata",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_useranswer",
                table: "useranswer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user",
                table: "user",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_question",
                table: "question",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_poll",
                table: "poll",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_personaldata",
                table: "personaldata",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_concept",
                table: "concept",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_answer",
                table: "answer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_answer_question_QuestionId",
                table: "answer",
                column: "QuestionId",
                principalTable: "question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_poll_concept_ConceptId",
                table: "poll",
                column: "ConceptId",
                principalTable: "concept",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_question_poll_PollId",
                table: "question",
                column: "PollId",
                principalTable: "poll",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_useranswer_answer_AnswerId",
                table: "useranswer",
                column: "AnswerId",
                principalTable: "answer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_useranswer_user_UserId",
                table: "useranswer",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userpersonaldata_personaldata_PersonalDataId",
                table: "userpersonaldata",
                column: "PersonalDataId",
                principalTable: "personaldata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userpersonaldata_user_UserId",
                table: "userpersonaldata",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userpoll_poll_PollId",
                table: "userpoll",
                column: "PollId",
                principalTable: "poll",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userpoll_user_UserId",
                table: "userpoll",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usertoken_user_UserId",
                table: "usertoken",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_answer_question_QuestionId",
                table: "answer");

            migrationBuilder.DropForeignKey(
                name: "FK_poll_concept_ConceptId",
                table: "poll");

            migrationBuilder.DropForeignKey(
                name: "FK_question_poll_PollId",
                table: "question");

            migrationBuilder.DropForeignKey(
                name: "FK_useranswer_answer_AnswerId",
                table: "useranswer");

            migrationBuilder.DropForeignKey(
                name: "FK_useranswer_user_UserId",
                table: "useranswer");

            migrationBuilder.DropForeignKey(
                name: "FK_userpersonaldata_personaldata_PersonalDataId",
                table: "userpersonaldata");

            migrationBuilder.DropForeignKey(
                name: "FK_userpersonaldata_user_UserId",
                table: "userpersonaldata");

            migrationBuilder.DropForeignKey(
                name: "FK_userpoll_poll_PollId",
                table: "userpoll");

            migrationBuilder.DropForeignKey(
                name: "FK_userpoll_user_UserId",
                table: "userpoll");

            migrationBuilder.DropForeignKey(
                name: "FK_usertoken_user_UserId",
                table: "usertoken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usertoken",
                table: "usertoken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userpoll",
                table: "userpoll");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userpersonaldata",
                table: "userpersonaldata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_useranswer",
                table: "useranswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_question",
                table: "question");

            migrationBuilder.DropPrimaryKey(
                name: "PK_poll",
                table: "poll");

            migrationBuilder.DropPrimaryKey(
                name: "PK_personaldata",
                table: "personaldata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_concept",
                table: "concept");

            migrationBuilder.DropPrimaryKey(
                name: "PK_answer",
                table: "answer");

            migrationBuilder.RenameTable(
                name: "usertoken",
                newName: "UserToken");

            migrationBuilder.RenameTable(
                name: "userpoll",
                newName: "UserPoll");

            migrationBuilder.RenameTable(
                name: "userpersonaldata",
                newName: "UserPersonalData");

            migrationBuilder.RenameTable(
                name: "useranswer",
                newName: "UserAnswer");

            migrationBuilder.RenameTable(
                name: "user",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "question",
                newName: "Question");

            migrationBuilder.RenameTable(
                name: "poll",
                newName: "Poll");

            migrationBuilder.RenameTable(
                name: "personaldata",
                newName: "PersonalData");

            migrationBuilder.RenameTable(
                name: "concept",
                newName: "Concept");

            migrationBuilder.RenameTable(
                name: "answer",
                newName: "Answer");

            migrationBuilder.RenameIndex(
                name: "IX_usertoken_UserId",
                table: "UserToken",
                newName: "IX_UserToken_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_userpoll_UserId",
                table: "UserPoll",
                newName: "IX_UserPoll_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_userpoll_PollId",
                table: "UserPoll",
                newName: "IX_UserPoll_PollId");

            migrationBuilder.RenameIndex(
                name: "IX_userpersonaldata_UserId",
                table: "UserPersonalData",
                newName: "IX_UserPersonalData_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_userpersonaldata_PersonalDataId",
                table: "UserPersonalData",
                newName: "IX_UserPersonalData_PersonalDataId");

            migrationBuilder.RenameIndex(
                name: "IX_useranswer_UserId",
                table: "UserAnswer",
                newName: "IX_UserAnswer_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_useranswer_AnswerId",
                table: "UserAnswer",
                newName: "IX_UserAnswer_AnswerId");

            migrationBuilder.RenameIndex(
                name: "IX_question_PollId",
                table: "Question",
                newName: "IX_Question_PollId");

            migrationBuilder.RenameIndex(
                name: "IX_poll_ConceptId",
                table: "Poll",
                newName: "IX_Poll_ConceptId");

            migrationBuilder.RenameIndex(
                name: "IX_answer_QuestionId",
                table: "Answer",
                newName: "IX_Answer_QuestionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserToken",
                table: "UserToken",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPoll",
                table: "UserPoll",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPersonalData",
                table: "UserPersonalData",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAnswer",
                table: "UserAnswer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Question",
                table: "Question",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Poll",
                table: "Poll",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonalData",
                table: "PersonalData",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Concept",
                table: "Concept",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Answer",
                table: "Answer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Answer_Question_QuestionId",
                table: "Answer",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Poll_Concept_ConceptId",
                table: "Poll",
                column: "ConceptId",
                principalTable: "Concept",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Poll_PollId",
                table: "Question",
                column: "PollId",
                principalTable: "Poll",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswer_Answer_AnswerId",
                table: "UserAnswer",
                column: "AnswerId",
                principalTable: "Answer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswer_User_UserId",
                table: "UserAnswer",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPersonalData_PersonalData_PersonalDataId",
                table: "UserPersonalData",
                column: "PersonalDataId",
                principalTable: "PersonalData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPersonalData_User_UserId",
                table: "UserPersonalData",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPoll_Poll_PollId",
                table: "UserPoll",
                column: "PollId",
                principalTable: "Poll",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPoll_User_UserId",
                table: "UserPoll",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserToken_User_UserId",
                table: "UserToken",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
