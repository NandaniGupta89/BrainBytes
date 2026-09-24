<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="QuizApp.WebForm1" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Programming Quiz Challenge</title>
    <style>
        body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #f0f2f5;
            margin: 0;
            padding: 0;
        }
        .quiz-container {
            max-width: 650px;
            margin: 50px auto;
            background: #ffffff;
            padding: 30px 40px;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }
        h2 {
            color: #2c3e50;
            text-align: center;
            margin-top: 0;
        }
        .tagline {
            text-align: center;
            color: #7f8c8d;
            font-size: 14px;
            margin-bottom: 25px;
        }
        .form-group {
            margin-bottom: 20px;
            text-align: left;
        }
        .field-label {
            display: block;
            font-weight: 600;
            color: #2c3e50;
            margin-bottom: 8px;
        }
        .text-input {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid #dcdfe3;
            border-radius: 6px;
            font-size: 15px;
            box-sizing: border-box;
        }
        .category-list {
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            gap: 10px;
        }
        .category-list input[type="radio"] {
            margin-right: 5px;
        }
        .category-list label {
            display: inline-flex;
            align-items: center;
            padding: 8px 16px;
            border: 1px solid #dcdfe3;
            border-radius: 20px;
            cursor: pointer;
            font-size: 14px;
        }
        .category-list label:hover {
            background-color: #f5f7fa;
        }
        .checkbox-row {
            margin: 18px 0;
            font-size: 14px;
            color: #2c3e50;
        }
        .greeting-text {
            color: #7f8c8d;
            font-size: 13px;
            margin-bottom: 12px;
            display: block;
        }
        .progress-wrap {
            margin-bottom: 18px;
        }
        .progress-track {
            background-color: #e6e9ec;
            border-radius: 20px;
            height: 12px;
            overflow: hidden;
        }
        .progress-fill {
            background-color: #3498db;
            height: 100%;
            border-radius: 20px;
            transition: width 0.3s ease;
        }
        .progress-text {
            font-size: 12px;
            color: #7f8c8d;
            text-align: right;
            margin-top: 5px;
        }
        .timer-box {
            display: inline-block;
            background-color: #fff3cd;
            color: #856404;
            padding: 6px 16px;
            border-radius: 20px;
            font-weight: 600;
            font-size: 14px;
            margin-bottom: 15px;
        }
        .question-text {
            font-size: 20px;
            font-weight: 600;
            color: #2c3e50;
            display: block;
            margin-bottom: 20px;
        }
        .rbl-options label {
            display: block;
            padding: 10px 15px;
            margin-bottom: 10px;
            border: 1px solid #dcdfe3;
            border-radius: 6px;
            cursor: pointer;
            transition: background-color 0.2s;
        }
        .rbl-options label:hover {
            background-color: #f5f7fa;
        }
        .btn-primary {
            background-color: #3498db;
            color: white;
            border: none;
            padding: 10px 25px;
            border-radius: 6px;
            font-size: 15px;
            cursor: pointer;
            margin-top: 10px;
        }
        .btn-primary:hover {
            background-color: #2980b9;
        }
        .error-text {
            color: #e74c3c;
            font-size: 13px;
            display: block;
            margin-top: 6px;
        }
        .result-box {
            text-align: center;
        }
        .result-greeting {
            display: block;
            font-size: 16px;
            color: #2c3e50;
            margin-top: 10px;
        }
        .score-text {
            font-size: 26px;
            font-weight: bold;
            color: #27ae60;
            display: block;
            margin: 12px 0;
        }
        .remark-text {
            display: block;
            font-size: 15px;
            color: #2c3e50;
            margin-bottom: 20px;
        }
        .review-item {
            padding: 12px;
            border-radius: 6px;
            margin-bottom: 10px;
            text-align: left;
        }
        .review-correct {
            background-color: #eafaf1;
            border-left: 4px solid #27ae60;
        }
        .review-wrong {
            background-color: #fdecea;
            border-left: 4px solid #e74c3c;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <asp:UpdatePanel ID="upMain" runat="server">
            <ContentTemplate>
                <div class="quiz-container">

                    <h2>💻 Programming Quiz Challenge</h2>

                    <!-- Home / Setup Panel -->
                    <asp:Panel ID="pnlSetup" runat="server">
                        <p class="tagline">🏠 Enter your name, pick a category, and test yourself with 10 questions.</p>

                        <div class="form-group">
                            <label class="field-label">👤 Your Name</label>
                            <asp:TextBox ID="txtUserName" runat="server" CssClass="text-input" placeholder="e.g. Anjali"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvUserName" runat="server"
                                ControlToValidate="txtUserName"
                                ErrorMessage="Please enter your name."
                                CssClass="error-text"
                                Display="Dynamic"
                                ValidationGroup="SetupGroup" />
                        </div>

                        <div class="form-group">
                            <label class="field-label">📚 Choose a Category</label>
                            <div class="category-list">
                                <asp:RadioButtonList ID="rblCategory" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal">
                                    <asp:ListItem Text="C" Value="C" />
                                    <asp:ListItem Text="Java" Value="Java" />
                                    <asp:ListItem Text="C#" Value="C#" />
                                    <asp:ListItem Text="Python" Value="Python" />
                                    <asp:ListItem Text="ASP.NET" Value="ASP.NET" />
                                </asp:RadioButtonList>
                            </div>
                            <asp:RequiredFieldValidator ID="rfvCategory" runat="server"
                                ControlToValidate="rblCategory"
                                ErrorMessage="Please select a category."
                                CssClass="error-text"
                                Display="Dynamic"
                                ValidationGroup="SetupGroup" />
                        </div>

                        <div class="checkbox-row">
                            <asp:CheckBox ID="chkTimer" runat="server" Text=" ⏱️ Enable 30-second timer per question" />
                        </div>

                        <asp:Button ID="btnStart" runat="server" Text="Start Quiz" CssClass="btn-primary"
                            ValidationGroup="SetupGroup" OnClick="btnStart_Click" />
                    </asp:Panel>

                    <!-- Quiz Panel -->
                    <asp:Panel ID="pnlQuiz" runat="server" Visible="false">

                        <asp:Label ID="lblUserGreeting" runat="server" CssClass="greeting-text"></asp:Label>

                        <div class="progress-wrap">
                            <div class="progress-track">
                                <asp:Panel ID="progressBarFill" runat="server" CssClass="progress-fill"></asp:Panel>
                            </div>
                            <div class="progress-text">
                                <asp:Label ID="lblQuestionNumber" runat="server"></asp:Label>
                            </div>
                        </div>

                        <asp:Panel ID="pnlTimer" runat="server" CssClass="timer-box" Visible="false">
                            ⏱️ Time left: <asp:Label ID="lblTimer" runat="server"></asp:Label>s
                        </asp:Panel>

                        <asp:Label ID="lblQuestion" runat="server" CssClass="question-text"></asp:Label>

                        <asp:RadioButtonList ID="rblOptions" runat="server" CssClass="rbl-options">
                        </asp:RadioButtonList>

                        <asp:RequiredFieldValidator ID="rfvOptions" runat="server"
                            ControlToValidate="rblOptions"
                            ErrorMessage="Please select an answer before continuing."
                            CssClass="error-text"
                            Display="Dynamic"
                            ValidationGroup="QuizGroup" />

                        <asp:Button ID="btnNext" runat="server" Text="Next" CssClass="btn-primary"
                            ValidationGroup="QuizGroup" OnClick="btnNext_Click" />

                        <asp:Timer ID="quizTimer" runat="server" Interval="1000" OnTick="quizTimer_Tick" Enabled="false" />

                    </asp:Panel>

                    <!-- Result Panel -->
                    <asp:Panel ID="pnlResult" runat="server" CssClass="result-box" Visible="false">
                        <h2>🏆 Quiz Completed!</h2>
                        <asp:Label ID="lblResultGreeting" runat="server" CssClass="result-greeting"></asp:Label>
                        <asp:Label ID="lblScore" runat="server" CssClass="score-text"></asp:Label>
                        <asp:Label ID="lblRemark" runat="server" CssClass="remark-text"></asp:Label>

                        <div>
                            <asp:Repeater ID="rptReview" runat="server">
                                <ItemTemplate>
                                    <div class='<%# (bool)Eval("IsCorrect") ? "review-item review-correct" : "review-item review-wrong" %>'>
                                        <strong>Q<%# Eval("QuestionNo") %>:</strong> <%# Eval("QuestionText") %><br />
                                        Your answer: <%# Eval("SelectedAnswer") %><br />
                                        Correct answer: <%# Eval("CorrectAnswer") %>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <asp:Button ID="btnRestart" runat="server" Text="🔄 Restart Quiz" CssClass="btn-primary"
                            OnClick="btnRestart_Click" CausesValidation="false" />
                    </asp:Panel>

                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
