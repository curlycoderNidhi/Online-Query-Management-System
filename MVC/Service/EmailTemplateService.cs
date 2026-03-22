using System;

namespace MVC.Service
{
    public class EmailTemplateService
    {
        public string GetOtpEmailTemplate(string otp, string purpose, string username)
        {
            // 🔥 Replace with your actual logo URL
            string logoUrl = @"https://github.com/curlycoderNidhi/Online-Query-Management-System/blob/main/Logo/logo.png?raw=true";

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                </head>

                <body style='margin:0;padding:0;background:#f4f6f9;font-family:Arial,sans-serif;'>

                    <div style='max-width:520px;margin:30px auto;background:#ffffff;border-radius:12px;
                                box-shadow:0 10px 30px rgba(0,0,0,0.1);overflow:hidden;border:1px solid #e5e7eb;'>

                        <!-- HEADER -->
                        <div style='background:linear-gradient(135deg,#0d6efd,#084298);padding:20px;text-align:center;color:#fff;'>

                            <img src='{logoUrl}' alt='Logo' style='height:50px;margin-bottom:10px;' />

                            <h2 style='margin:0;font-size:20px;font-weight:700;'>
                                Query Management System
                            </h2>
                        </div>

                        <!-- BODY -->
                        <div style='padding:25px;'>

                            <p style='font-size:14px;color:#333;margin-bottom:10px;'>Hello, {username}!</p>

                            <p style='font-size:14px;color:#333;margin-bottom:20px;'>
                                Your One-Time Password (OTP) for <b>{purpose}</b> is:
                            </p>

                            <!-- OTP BOX -->
                            <div style='text-align:center;margin:25px 0;'>
                                <span style='display:inline-block;background:#f1f5ff;color:#0d6efd;
                                            font-size:32px;font-weight:800;letter-spacing:8px;
                                            padding:12px 25px;border-radius:10px;border:1px dashed #0d6efd;'>
                                    {otp}
                                </span>
                            </div>

                            <p style='font-size:13px;color:#555;margin-bottom:15px;'>
                                This OTP is valid for a short time. Please do not share it with anyone.
                            </p>

                            <p style='font-size:13px;color:#555;'>
                                If you did not request this, you can safely ignore this email.
                            </p>

                        </div>

                        <!-- FOOTER -->
                        <div style='background:#f9fafb;padding:15px;text-align:center;
                                    font-size:12px;color:#999;border-top:1px solid #eee;'>

                            © {DateTime.Now.Year} Query Management System <br/>
                            All rights reserved

                        </div>

                    </div>

                </body>
                </html>";
        }
        public string GetWelcomeEmailTemplate(string username)
        {
            string logoUrl = "https://github.com/curlycoderNidhi/Online-Query-Management-System/blob/main/Logo/logo.png?raw=true";

            return $@"
    <div style='font-family:Segoe UI; background:#f4f6f9; padding:30px'>
        
        <div style='max-width:600px; margin:auto; background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 5px 20px rgba(0,0,0,0.1)'>

            <!-- Header -->
            <div style='background:linear-gradient(135deg,#0d6efd,#084298); padding:20px; text-align:center'>
                <img src='{logoUrl}' alt='Logo' style='height:60px; margin-bottom:10px'/>
                <h2 style='color:#fff; margin:0'>Query Management System</h2>
            </div>

            <!-- Body -->
            <div style='padding:30px; text-align:center'>
                <h3 style='margin-bottom:10px'>Welcome, {username} 🎉</h3>

                <p style='color:#555; font-size:15px'>
                    Your account has been successfully created.
                </p>

                <p style='color:#555; font-size:15px'>
                    You can now login and start managing your queries easily.
                </p>

                <a href='http://localhost:5141/user/login'
                   style='display:inline-block; margin-top:20px; padding:12px 25px; background:#0d6efd; color:#fff; text-decoration:none; border-radius:6px; font-weight:600'>
                    Login Now
                </a>
            </div>

            <!-- Footer -->
            <div style='background:#f1f1f1; padding:15px; text-align:center; font-size:12px; color:#888'>
                © 2026 Query Management System
            </div>

        </div>

    </div>";
        }
        public string GetQueryResolvedTemplate(string username, string queryTitle)
        {
           string logoUrl = "https://github.com/curlycoderNidhi/Online-Query-Management-System/blob/main/Logo/logo.png?raw=true";

            return $@"
    <div style='font-family:Segoe UI; background:#f4f6f9; padding:30px'>
        
        <div style='max-width:600px; margin:auto; background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 5px 20px rgba(0,0,0,0.1)'>

            <!-- Header -->
            <div style='background:linear-gradient(135deg,#0d6efd,#084298); padding:20px; text-align:center'>
                <img src='{logoUrl}' style='height:60px; margin-bottom:10px'/>
                <h2 style='color:#fff; margin:0'>Query Management System</h2>
            </div>

            <!-- Body -->
            <div style='padding:30px'>
                
                <h3 style='margin-bottom:10px'>Hello {username}, 👋</h3>

                <p style='color:#555; font-size:15px'>
                    We’re happy to inform you that your query has been successfully resolved.
                </p>

                <div style='margin:20px 0; padding:15px; background:#f1f5ff; border-left:4px solid #0d6efd; border-radius:6px'>
                    <strong>Query Title:</strong><br/>
                    {queryTitle}
                </div>

                <p style='color:#555; font-size:15px'>
                    If you have any further questions or need additional assistance, feel free to raise a new query.
                </p>

                <a href='http://localhost:5141/user/dashboard'
                   style='display:inline-block; margin-top:20px; padding:12px 25px; background:#0d6efd; color:#fff; text-decoration:none; border-radius:6px; font-weight:600'>
                    View Dashboard
                </a>

            </div>

            <!-- Footer -->
            <div style='background:#f1f1f1; padding:15px; text-align:center; font-size:12px; color:#888'>
                © 2026 Query Management System
            </div>

        </div>

    </div>";
        }
    }

}