using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PaymentsApplication.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentsApplication.Controllers
{
    public class HomeController : Controller
    {
        public static List<string> UsersOfSessionsList=new List<string>();
        // عرض الصفحة الرئيسية
        [HttpGet]
         
        public IActionResult Index()
        {
            return View("Login");
        }
        public IActionResult LoginPage()
        { return View("Login"); }

        public IActionResult RegisterPage()
        {
            return View("Register");
        }
        public IActionResult AddMoneyPage()
        {

            string PhoneNumber = HttpContext.Session.GetString("UserPhone");

            if (String.IsNullOrEmpty(PhoneNumber))
            {
                return View("Login");
            }


            userRepo userRepo1 = new userRepo();
            User user = new User();
            user = userRepo1.getUserDataByPhoneNumber(PhoneNumber);
            ViewBag.UserBalance = user.Balance;
            return View("AddMoney");
        }
        public IActionResult SendMoneyPage()
        {
            string PhoneNumber = HttpContext.Session.GetString("UserPhone");

            if (String.IsNullOrEmpty(PhoneNumber))
            {
                return View("Login");
            }
            userRepo userRepo1= new userRepo();
            User user = new User();
            user=userRepo1.getUserDataByPhoneNumber(PhoneNumber);
            ViewBag.UserBalance=user.Balance;
            return View("SendMoney");
        }
        public IActionResult ChangePasswordPage()
        {
            string PhoneNumber = HttpContext.Session.GetString("UserPhone");

            if (String.IsNullOrEmpty(PhoneNumber))
            {
                return View("Login");
            }
            return View("ChangePassword");
        }
        public IActionResult DashboardPage()
        {
            string PhoneNumber = HttpContext.Session.GetString("UserPhone");

            if (String.IsNullOrEmpty(PhoneNumber))
            {
                return View("Login");
            }
            userRepo userRepo1=new userRepo();
            User user=new User();
            user= userRepo1.getUserDataByPhoneNumber(PhoneNumber);
            ViewBag.UserPhonenum = user.PhoneNumber;

            ViewBag.UserName=user.UserName;
            ViewBag.UserBalance=user.Balance;
            transactionRepo transactionRepo1 =new transactionRepo();
            ViewBag.SumOfAddMoney = transactionRepo1.GetSumOfAddMoney(PhoneNumber);
            ViewBag.SumOfSendMoney = transactionRepo1.GetSumOfSendMoney(PhoneNumber);
            ViewBag.SumOfReceiveMoney = transactionRepo1.GetSumOfReceiveMoney(PhoneNumber);
            return View("Dashboard"); }
        public IActionResult TransactionsHistoryPage()
        {
            string PhoneNumber = HttpContext.Session.GetString("UserPhone");

            if (String.IsNullOrEmpty(PhoneNumber))
            {
                return View("Login");
            }
            return View("TransactionsHistory");
        }
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length == 11)
            { return true; }
            else { return false; }
        }
        public bool IsValidAmount(decimal Amount) 
        {
        if(!string.IsNullOrWhiteSpace(Amount.ToString())&& Amount>0)
           { 
                return true; 
           }
        else 
            {
                return false;
            }
        }
        public bool IsValidBalance(decimal Balance)
        {
            if (!string.IsNullOrWhiteSpace(Balance.ToString()) && Balance > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // يتم استدعاء هذا الميثود عند الضغط على الزر
        [HttpPost]
        public IActionResult Register(string PhoneNumber , string UserName, string Password,string ConfirmPassword, decimal Balance,string UserEmail )
        {

            if (!IsValidPhoneNumber(PhoneNumber))
            {
                ViewBag.RegisterResponse = "Phone number must be 11 digits";
                return View();

            }
            if (!IsValidBalance(Balance)) 
            { ViewBag.RegisterResponse = "Invalid balance";
                return View("Register");
            }
            if(ConfirmPassword!= Password)
            {
                ViewBag.RegisterResponse = "you entered confirm password not the same of new password";
                return View("Register");
            }
            User user = new User();
            user.PhoneNumber =PhoneNumber;
            user.UserName =UserName;
            user.Password =Password;
            user.Balance =Balance;
            user.UserEmail= UserEmail;

            userRepo userRepo1 = new userRepo();
            User checkUser = userRepo1.getUserDataByPhoneNumber(user.PhoneNumber);
            if (checkUser == null)
            {
                bool added = userRepo1.InsertUser(user);
                ViewBag.UserBalance = Balance;
                ViewBag.RegisterResponse = "Registration successful";
                
                ViewBag.UserPhonenum = user.PhoneNumber;
                ViewBag.UserName = user.UserName;
                return View("Login");
            }
            else
            {
                ViewBag.RegisterResponse = "user allready exsit";
                return View("Register");
            }
            
        }   
        //test API
        public class LoginRequest
        {
            [JsonPropertyName("Phone")]
            public string PhoneNumber { get; set; }

            [JsonPropertyName("Pass")]
            public string Password { get; set; }
        }
        public class LoginResponse
        {
            public string PhoneNumber { get; set; }
            public string Username { get; set; }
            public bool Success { get; set; }
            public decimal Balance { get; set; }
            public string Message { get; set; }
            public decimal SumOfAddMoney { get; set; }
            public decimal SumOfSendMoney { get; set; }
            public decimal SumOfReceiveMoney { get; set; }
        }
        public class RegisterRequest
        {
            [JsonPropertyName("Phone")]
            public string PhoneNumber { get; set; }

            [JsonPropertyName("Name")]
            public string UserName { get; set; }

            [JsonPropertyName("Pass")]
            public string Password { get; set; }

            [JsonPropertyName("confirmPass")]
            public string ConfirmPassword { get; set; }

            [JsonPropertyName("Balance")]
            public decimal Balance { get; set; }

        }
        public class RegisterResponse
        {
            public string PhoneNumber { get; set; }
            public string Username { get; set; }
            public bool Success { get; set; }
            public decimal Balance { get; set; }
            public string Message { get; set; }
        }
        [HttpPost]
        [Route("/api/users/login")]
        public LoginResponse Login([FromBody] LoginRequest loginRequest)
        {
            LoginResponse loginResponse = new LoginResponse();

            if (!IsValidPhoneNumber(loginRequest.PhoneNumber))
            {
                loginResponse.Success = false;

                loginResponse.Message = "Phone number must be 11 digits";
                return loginResponse;

            }
            userRepo userRepo1 = new userRepo();
            User user = new User();
            user = userRepo1.getUserDataByPhoneNumber(loginRequest.PhoneNumber);

            if (user != null && user.Password == loginRequest.Password)
            {
                foreach (string session in UsersOfSessionsList)
                {
                    if (session == user.PhoneNumber)
                    {
                        loginResponse.Message = "Login failed. This account is already logged in elsewhere";
                        loginResponse.Success = false;
                        return loginResponse;
                    }

                }
                UsersOfSessionsList.Add(user.PhoneNumber);
                ViewBag.UserBalance = user.Balance;

                HttpContext.Session.SetString("UserPhone", user.PhoneNumber);
                ViewBag.UserPhonenum = user.PhoneNumber;
                ViewBag.UserName = user.UserName;
                transactionRepo transactionRepo1 = new transactionRepo();
                ViewBag.SumOfAddMoney = transactionRepo1.GetSumOfAddMoney(loginRequest.PhoneNumber);
                ViewBag.SumOfSendMoney = transactionRepo1.GetSumOfSendMoney(loginRequest.PhoneNumber);
                ViewBag.SumOfReceiveMoney = transactionRepo1.GetSumOfReceiveMoney(loginRequest.PhoneNumber);
                //return View("Dashboard");
                loginResponse.PhoneNumber=user.PhoneNumber;
                loginResponse.Success =true;
                loginResponse.Balance = user.Balance;
                loginResponse.Message = "Login success";
                loginResponse.Username = user.UserName;
                loginResponse.SumOfAddMoney = transactionRepo1.GetSumOfAddMoney(loginRequest.PhoneNumber);
                loginResponse.SumOfSendMoney = transactionRepo1.GetSumOfSendMoney(loginRequest.PhoneNumber);
                loginResponse.SumOfReceiveMoney = transactionRepo1.GetSumOfReceiveMoney(loginRequest.PhoneNumber);
                return loginResponse;
            }
            else
            {
                loginResponse.Message = "wrong user name or password";
                loginResponse.Success= false;

                return loginResponse;
            }



        }


        [HttpGet]
        [Route("/Api/Users/Login")]
        public List<User> users()
        { 
            userRepo userRepo1 = new userRepo();

            return userRepo1.getallUsers();
        }
        
        [HttpPost]
        [Route("/Api/Users/Register")]

        public RegisterResponse Register([FromBody] RegisterRequest registerRequest)
        {

            RegisterResponse registerResponse = new RegisterResponse();

            if (!IsValidPhoneNumber(registerRequest.PhoneNumber))
            {
                registerResponse.Success= false;
                registerResponse.Message = "Phone number must be 11 digits";
                return registerResponse;

            }
            if (!IsValidBalance(registerRequest.Balance))
            {
                registerResponse.Success = false;
                registerResponse.Message = "Invalid balance";
                return registerResponse;
            }
            if (registerRequest.ConfirmPassword != registerRequest.Password)
            {
                registerResponse.Success = false;
                registerResponse.Message = "you entered confirm password not the same of new password";
                return registerResponse;
            }
            User user = new User();
            user.PhoneNumber = registerRequest.PhoneNumber;
            user.UserName = registerRequest.UserName;
            user.Password = registerRequest.Password;
            user.Balance = registerRequest.Balance;
            ;

            userRepo userRepo1 = new userRepo();
            User checkUser = userRepo1.getUserDataByPhoneNumber(user.PhoneNumber);
            if (checkUser == null)
            {
                bool added = userRepo1.InsertUser(user);
                ViewBag.UserBalance = registerRequest.Balance;

                ViewBag.UserPhonenum = registerRequest.PhoneNumber;
                ViewBag.UserName = registerRequest.UserName;
                registerResponse.PhoneNumber = registerRequest.PhoneNumber;
                registerResponse.Username = registerRequest.UserName;
                registerResponse.Balance = registerRequest.Balance;
                registerResponse.Success = true;
                registerResponse.Message = "Register successful";
                return registerResponse;
            }
            else
            {
                registerResponse.Success = false;
                registerResponse.Message = "user allready exsit";
                return registerResponse;
            }

        }
        public class ChangePasswordRequest
        {
            [JsonPropertyName("Phone")]
            public string PhoneNumber { get; set; }

            [JsonPropertyName("CurrentPass")]
            public string CurrentPassword { get; set; }
            [JsonPropertyName("NewPass")]
            public string NewPassword { get; set; }
            [JsonPropertyName("ConfirmNewPass")]
            public string ConfirmNewPassword { get; set; }
        }
        public class ChangePasswordResponse
        {
            public string PhoneNumber { get; set; }
            public string UserName { get; set; }

            public bool Success { get; set; }
            public decimal Balance { get; set; }
            public string Message { get; set; }
        }
        [HttpPost]
        [Route("/Api/Users/ChangePassword")]
        public ChangePasswordResponse ChangePassword([FromBody]ChangePasswordRequest changePasswordRequest)
        {
            ChangePasswordResponse changePasswordResponse = new ChangePasswordResponse();
            if (!IsValidPhoneNumber(changePasswordRequest.PhoneNumber))
            {
                changePasswordResponse.Success= false;
                changePasswordResponse.Message = "Phone number must be 11 digits";
                return changePasswordResponse;

            }
            
            if (changePasswordRequest.PhoneNumber != changePasswordRequest.PhoneNumber)
            {
                changePasswordResponse.Success= false;
                changePasswordResponse.Message = "Phone number must be matches login phone number";
                return changePasswordResponse;
            }
            userRepo userRepo1 = new userRepo();
            User user = userRepo1.getUserDataByPhoneNumber(changePasswordRequest.PhoneNumber);
            if (user != null && user.Password == changePasswordRequest.CurrentPassword)
            {
                if (changePasswordRequest.NewPassword == user.Password)
                {
                    changePasswordResponse.Success= false;
                    changePasswordResponse.Message = "Change the password to a new one";
                    return changePasswordResponse;
                }
                if (changePasswordRequest.NewPassword != changePasswordRequest.ConfirmNewPassword)
                {
                    changePasswordResponse.Success = false;
                    changePasswordResponse.Message = "you entered confirm new password not the same of new password";
                    return changePasswordResponse;
                }
                else
                {

                    user.Password = changePasswordRequest.NewPassword;
                    userRepo1.updateUser(user);
                    changePasswordResponse.Success = true;
                    changePasswordResponse.Message = "Your password has been successfully changed";
                    changePasswordResponse.PhoneNumber= changePasswordRequest.PhoneNumber;
                        changePasswordResponse.Balance=user.Balance;
                        changePasswordResponse.UserName=user.UserName;
                        
                    return changePasswordResponse;
                }
            }
            else
            {
                changePasswordResponse.Success = false;
               changePasswordResponse.Message = "Wrong Current Password";
                return changePasswordResponse;
            }
        }
        public class AddMoneyRequest
        {
            [JsonPropertyName("Phone")]
            public string PhoneNumber { get; set; }
            [JsonPropertyName("Amount")]
            public decimal Amount { get; set; }
        }
        public class AddMoneyResponse
        {
            public string PhoneNumber { get; set; }
            public string UserName { get; set; }

            public bool Success { get; set; }
            public decimal Balance { get; set; }
            public string Message { get; set; }
        }
        [HttpPost]
        [Route("/Api/Users/AddMoney")]
        public AddMoneyResponse AddMoney([FromBody] AddMoneyRequest addMoneyRequest)
        {
            AddMoneyResponse addMoneyResponse = new AddMoneyResponse();

            if (String.IsNullOrEmpty(addMoneyRequest.PhoneNumber))
            {
                addMoneyResponse.Message = "String null or empty";
                addMoneyResponse.Success = false;
                return addMoneyResponse;
            }


            if (!IsValidPhoneNumber(addMoneyRequest.PhoneNumber))
            {
                addMoneyResponse.Success=false;
                addMoneyResponse.Message = "Phone number must be 11 digits";

                return addMoneyResponse;

            }
            if (!IsValidAmount(addMoneyRequest.Amount))
            {
                addMoneyResponse.Success = false;
                addMoneyResponse.Message= "Invalid amount";
                return addMoneyResponse;

            }

            userRepo userRepo1 = new userRepo();
            User user = new User();
            user = userRepo1.getUserDataByPhoneNumber(addMoneyRequest.PhoneNumber);
            if (user != null)
            {
                user.Balance = user.Balance + addMoneyRequest.Amount;
                userRepo1.updateUser(user);
                user = userRepo1.getUserDataByPhoneNumber(addMoneyRequest.PhoneNumber);
                Transaction transaction = new Transaction();
                transaction.SenderId = user.UserId;
                transaction.ReceiverId = user.UserId;
                transaction.SenderPhoneNumber = user.PhoneNumber;
                transaction.ReceiverPhoneNumber = user.PhoneNumber;
                transaction.Amount = addMoneyRequest.Amount;
                transaction.TransactionType = "add money";
                transactionRepo transactionRepo1 = new transactionRepo();
                transactionRepo1.insertTransaction(transaction);
                ViewBag.UserBalance = user.Balance;
                addMoneyResponse.Success=true;
                addMoneyResponse.Message = "The operation was successful " + "your current balance: " + user.Balance.ToString();
                addMoneyResponse.PhoneNumber=addMoneyRequest.PhoneNumber;
                addMoneyResponse.UserName=user.UserName;
                addMoneyResponse.Balance=user.Balance;
                return addMoneyResponse;

            }
            else
            {
                addMoneyResponse.Success=false;
                addMoneyResponse.Message = "user not found";
                return addMoneyResponse;

            }
        }
        public class SendMoneyRequest
        {
            [JsonPropertyName("SenderPhone")]
            public string SenderPhoneNumber { get; set; }
            [JsonPropertyName("ReceiverPhone")]
            public string ReceiverPhoneNumber { get; set; }
            [JsonPropertyName("Amount")]
            public decimal Amount { get; set; }
        }
        public class SendMoneyResponse
        {
            public string PhoneNumber { get; set; }
            public string UserName { get; set; }

            public bool Success { get; set; }
            public decimal Balance { get; set; }
            public string Message { get; set; }
        }
        [HttpPost]
        [Route("/Api/Users/SendMoney")]
        public SendMoneyResponse sendMoney([FromBody] SendMoneyRequest sendMoneyRequest)
        {
            SendMoneyResponse sendMoneyResponse = new SendMoneyResponse();
            userRepo userRepo2 = new userRepo();
            /*string SenderPhoneNum = HttpContext.Session.GetString("UserPhone");
            User user2 = new User();
            
            user2 = userRepo2.getUserDataByPhoneNumber(SenderPhoneNum);
            ViewBag.UserBalance = user2.Balance;*/
            if (!IsValidPhoneNumber(sendMoneyRequest.SenderPhoneNumber) || !IsValidPhoneNumber(sendMoneyRequest.ReceiverPhoneNumber))
            {
                sendMoneyResponse.Success=false;
                
                sendMoneyResponse.Message = "Phone number must be 11 digits";

                return sendMoneyResponse;

            }

            if (!IsValidAmount(sendMoneyRequest.Amount))
            {
                sendMoneyResponse.Success = false;
                sendMoneyResponse.Message = "Invalid amount";
                return sendMoneyResponse;

            }
            if (sendMoneyRequest.SenderPhoneNumber == sendMoneyRequest.ReceiverPhoneNumber)
            {
                sendMoneyResponse.Success = false;
                sendMoneyResponse.Message = "You cannot send money to yourself";
                return sendMoneyResponse;
            }
            userRepo userRepo1 = new userRepo();
            User sender = new User();
            sender = userRepo1.getUserDataByPhoneNumber(sendMoneyRequest.SenderPhoneNumber);
            if (sender == null)
            {
                sendMoneyResponse.Success = false;
                sendMoneyResponse.Message = "sender not found";
                return sendMoneyResponse;
            }

            if (sender.Balance < sendMoneyRequest.Amount)
            {
                sendMoneyResponse.Success = false;
                sendMoneyResponse.Message = "Your balance is insufficient";
                return sendMoneyResponse;

            }
            User receiver = new User();

            receiver = userRepo1.getUserDataByPhoneNumber(sendMoneyRequest.ReceiverPhoneNumber);
            if (receiver == null)
            {
                sendMoneyResponse.Success = false;
                
                sendMoneyResponse.Message= "receiver not found";
                return sendMoneyResponse;

            }
            else
            {
                //approch1
                //decimal newSenderBalance = sender.Balance - amount;
                //userRepo1.updateUserBalance(senderPhonenum, newSenderBalance);

                //approch2
                sender.Balance = sender.Balance - sendMoneyRequest.Amount;
                userRepo1.updateUser(sender);

                //decimal newReceiverBalance = receiver.Balance + amount;
                receiver.Balance = receiver.Balance + sendMoneyRequest.Amount;
                userRepo1.updateUser(receiver);


                //insert transactions into data base
                Transaction transaction = new Transaction();
                transaction.SenderId = sender.UserId;
                transaction.ReceiverId = receiver.UserId;
                transaction.SenderPhoneNumber = sender.PhoneNumber;
                transaction.ReceiverPhoneNumber = receiver.PhoneNumber;
                transaction.Amount = sendMoneyRequest.Amount;
                transaction.TransactionType = "send money";

                transactionRepo transactionRepo1 = new transactionRepo();
                transactionRepo1.insertTransaction(transaction);
                ViewBag.UserBalance = sender.Balance;
                sendMoneyResponse.Success = true;
                sendMoneyResponse.Message= "The operation was successful " + "Your balance now: " + sender.Balance;
                sendMoneyResponse.Balance = sender.Balance;
                sendMoneyResponse.UserName=sender.UserName;
                sendMoneyResponse.PhoneNumber = sendMoneyRequest.SenderPhoneNumber;
                return sendMoneyResponse;
            }


        }
        public class DisplayTransactionsHistoryResponse
        {
            
            public bool Success {  get; set; }
            public string Message { get; set; }
            public List<Transaction> AllTransactionsList { get; set; } = new List<Transaction>();
        }
        [HttpPost]
        [Route("/Api/Transactions/DisplayTransactionsHistory")]
        public DisplayTransactionsHistoryResponse DisplayTransactionsHistory([FromBody] string PhoneNumber)
        {
            DisplayTransactionsHistoryResponse displayTransactionsHistoryResponse = new DisplayTransactionsHistoryResponse();

            if (!IsValidPhoneNumber(PhoneNumber))
            {
                displayTransactionsHistoryResponse.Success = false;
                displayTransactionsHistoryResponse.Message = "Phone number must be 11 digits";
                return displayTransactionsHistoryResponse;

            }
            transactionRepo transactionRepo1 = new transactionRepo();
            displayTransactionsHistoryResponse.Success = true;
            
            displayTransactionsHistoryResponse.AllTransactionsList = transactionRepo1.GetTransactionsHistory(PhoneNumber);
            



            return displayTransactionsHistoryResponse;

        }
        public class DisplayTransactionsHistoryFromDateRequest
        {
            [JsonPropertyName("Phone")]
            public string PhoneNumber { get; set; }
            [JsonPropertyName("TransactionStartDate")]
            public DateTime TransactionStartDate { get; set; }
            [JsonPropertyName("TransactionEndDate")]
            public DateTime TransactionEndDate { get; set; }
        }
        public class DisplayTransactionsHistoryFromDateResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<Transaction> transactionsFromDateList { get; set; } = new List<Transaction>();
        }
        [HttpPost]
        [Route("/Api/Transactions/DisplayTransactionsHistoryFromDate")]
        public DisplayTransactionsHistoryFromDateResponse DisplayTransactionsHistoryFromDate([FromBody] DisplayTransactionsHistoryFromDateRequest displayTransactionsHistoryFromDateRequest)
        {
            DisplayTransactionsHistoryFromDateResponse displayTransactionsHistoryFromDateResponse = new DisplayTransactionsHistoryFromDateResponse();

            transactionRepo transactionRepo1 = new transactionRepo();

            displayTransactionsHistoryFromDateResponse.Success = true;

            displayTransactionsHistoryFromDateResponse.transactionsFromDateList = transactionRepo1.GetTransactionsHistoryFromDate(displayTransactionsHistoryFromDateRequest.PhoneNumber, displayTransactionsHistoryFromDateRequest.TransactionStartDate, displayTransactionsHistoryFromDateRequest.TransactionEndDate);

            
            return displayTransactionsHistoryFromDateResponse;

        }

        //end of API
        public IActionResult Login(string PhoneNumber,string password)
        {
            if(!IsValidPhoneNumber(PhoneNumber))
            {
                ViewBag.LoginResponse = "Phone number must be 11 digits";
                return View();
            
            }
            userRepo userRepo1 = new userRepo();
            User user = new User();
            user = userRepo1.getUserDataByPhoneNumber(PhoneNumber);

            if (user != null && user.Password == password)
            {
                foreach(string session in UsersOfSessionsList)
                {
                    if (session==user.PhoneNumber)
                    {
                        ViewBag.LoginResponse = "Login failed. This account is already logged in elsewhere";
                        
                        return View("Login");
                    }

                }
                UsersOfSessionsList.Add(user.PhoneNumber);
                ViewBag.UserBalance = user.Balance;

                HttpContext.Session.SetString("UserPhone", user.PhoneNumber);
                ViewBag.UserPhonenum = user.PhoneNumber;
                ViewBag.UserName = user.UserName;
                transactionRepo transactionRepo1= new transactionRepo();
                ViewBag.SumOfAddMoney = transactionRepo1.GetSumOfAddMoney(PhoneNumber);
                ViewBag.SumOfSendMoney = transactionRepo1.GetSumOfSendMoney(PhoneNumber);
                ViewBag.SumOfReceiveMoney = transactionRepo1.GetSumOfReceiveMoney(PhoneNumber);
                return View("Dashboard");
            }
            else
            {
                ViewBag.LoginResponse ="wrong user name or password";

                
                return View("Login");
            }

            

        }
        public IActionResult ChangePassword(string PhoneNumber, string CurrentPassword, string NewPassword,string ConfirmNewPassword)
        {
            
            if (!IsValidPhoneNumber(PhoneNumber))
            {
                ViewBag.ChangePasswordResponse = "Phone number must be 11 digits";
                return View();

            }
            string CurrentUserPhoneNumber = HttpContext.Session.GetString("UserPhone");
            if(CurrentUserPhoneNumber != PhoneNumber)
            {
                ViewBag.ChangePasswordResponse = "Phone number must be matches login phone number";
                return View();
            }
            userRepo userRepo1 = new userRepo();
            User user = userRepo1.getUserDataByPhoneNumber(PhoneNumber);
            if (user != null && user.Password == CurrentPassword)
            {
                if (NewPassword == user.Password)
                {
                    ViewBag.ChangePasswordResponse = "Change the password to a new one";
                    return View("ChangePassword");
                }
                if (NewPassword != ConfirmNewPassword)
                {

                    ViewBag.ChangePasswordResponse = "you entered confirm new password not the same of new password";
                    return View("ChangePassword");
                }
                else
                {
                    
                    user.Password = NewPassword;
                    userRepo1.updateUser(user);
                    ViewBag.ChangePasswordResponse = "Your password has been successfully changed";
                    ViewBag.UserPhonenum = user.PhoneNumber;
                    ViewBag.UserName = user.UserName;
                    return View("Dashboard");
                }
            }
            else 
            { ViewBag.ChangePasswordResponse = "Wrong Current Password";
                return View("ChangePassword");
            }
        }
        
        public IActionResult AddMoney(decimal Amount)
        {
            string PhoneNumber = HttpContext.Session.GetString("UserPhone");

            if(String.IsNullOrEmpty(PhoneNumber))
            {
                return View("Login");
            }

            
            if (!IsValidPhoneNumber(PhoneNumber))
            {
                
                ViewBag.AddMoneyResponse = "Phone number must be 11 digits";
                
                return View();

            }
            if (!IsValidAmount(Amount))
            {
                ViewBag.AddMoneyResponse = "Invalid amount";
                return View ("AddMoney");
                 
            }

            userRepo userRepo1 = new userRepo();
            User user = new User();
            user = userRepo1.getUserDataByPhoneNumber(PhoneNumber);
            if (user != null)
            {
                user.Balance = user.Balance + Amount;
                userRepo1.updateUser(user);
                user = userRepo1.getUserDataByPhoneNumber(PhoneNumber);
                Transaction transaction = new Transaction();
                transaction.SenderId = user.UserId;
                transaction.ReceiverId = user.UserId;
                transaction.SenderPhoneNumber = user.PhoneNumber;
                transaction.ReceiverPhoneNumber = user.PhoneNumber;
                transaction.Amount = Amount;
                transaction.TransactionType = "add money";
                transactionRepo transactionRepo1 = new transactionRepo();
                transactionRepo1.insertTransaction(transaction);
                ViewBag.UserBalance = user.Balance;
                ViewBag.AddMoneyResponse = "The operation was successful "+"your current balance: " + user.Balance.ToString();
                
                return View("AddMoney");
                
            }
            else
            {
                ViewBag.AddMoneyResponse = "user not found";
                return View("AddMoney");
                
            }
        }
        public IActionResult sendMoney(string ReceiverPhoneNum, decimal Amount)
        {
            string SenderPhoneNum = HttpContext.Session.GetString("UserPhone");
            User user2 = new User();
            userRepo userRepo2 = new userRepo();
            user2 = userRepo2.getUserDataByPhoneNumber(SenderPhoneNum);
            ViewBag.UserBalance = user2.Balance;
            if (!IsValidPhoneNumber(SenderPhoneNum) || !IsValidPhoneNumber(ReceiverPhoneNum))
            {
                ViewBag.SendMoneyResponse = "Phone number must be 11 digits";
                
                return View();

            }
            
            if (!IsValidAmount(Amount))
            {
                ViewBag.SendMoneyResponse = "Invalid amount";
                return View("SendMoney");

            }
            if(SenderPhoneNum==ReceiverPhoneNum)
            {
                ViewBag.SendMoneyResponse = "You cannot send money to yourself";
                return View("SendMoney");
            }
            userRepo userRepo1 = new userRepo();
            User sender = new User();
            sender = userRepo1.getUserDataByPhoneNumber(SenderPhoneNum);
            if (sender == null)
            {
                ViewBag.SendMoneyResponse = "sender not found";
                return View("SendMoney");
            }

            if (sender.Balance < Amount)
            {
                ViewBag.SendMoneyResponse = "Your balance is insufficient";
                return View("SendMoney");
                
            }
            User receiver = new User();

            receiver = userRepo1.getUserDataByPhoneNumber(ReceiverPhoneNum);
            if (receiver == null)
            {
                ViewBag.SendMoneyResponse = "receiver not found";
                return View("SendMoney");
                
            }
            else
            {
                //approch1
                //decimal newSenderBalance = sender.Balance - amount;
                //userRepo1.updateUserBalance(senderPhonenum, newSenderBalance);

                //approch2
                sender.Balance = sender.Balance - Amount;
                userRepo1.updateUser(sender);

                //decimal newReceiverBalance = receiver.Balance + amount;
                receiver.Balance = receiver.Balance + Amount;
                userRepo1.updateUser(receiver);


                //insert transactions into data base
                Transaction transaction = new Transaction();
                transaction.SenderId = sender.UserId;
                transaction.ReceiverId = receiver.UserId;
                transaction.SenderPhoneNumber = sender.PhoneNumber;
                transaction.ReceiverPhoneNumber = receiver.PhoneNumber;
                transaction.Amount = Amount;
                transaction.TransactionType = "send money";

                transactionRepo transactionRepo1 = new transactionRepo();
                transactionRepo1.insertTransaction(transaction);
                ViewBag.UserBalance=sender.Balance;
                ViewBag.SendMoneyResponse = "The operation was successful "+"Your balance now: "+sender.Balance;
                return View("SendMoney");
            }


        }
        public IActionResult DisplayTransactionsHistory()
        {
            string PhoneNumber = HttpContext.Session.GetString("UserPhone");

            if (!IsValidPhoneNumber(PhoneNumber))
            {
                ViewBag.AddMoneyResponse = "Phone number must be 11 digits";
                return View("TransactionsHistory");

            }
            transactionRepo transactionRepo1= new transactionRepo();
            List<Transaction> AllTransactionsList = transactionRepo1.GetTransactionsHistory(PhoneNumber);
            

            ViewBag.AllTransactions = AllTransactionsList;
            return View("TransactionsHistory");
            
        }
        public IActionResult DisplayTransactionsHistoryFromDate(DateTime TransactionStartDate, DateTime TransactionEndDate)
        {
            string PhoneNumber = HttpContext.Session.GetString("UserPhone");

            transactionRepo transactionRepo1 = new transactionRepo();
            List<Transaction> transactionsFromDateList = transactionRepo1.GetTransactionsHistoryFromDate(PhoneNumber, TransactionStartDate, TransactionEndDate);

            ViewBag.AllTransactions = transactionsFromDateList;
            return View("TransactionsHistory");

        }
        public IActionResult Logout()
        {
            UsersOfSessionsList.Remove(HttpContext.Session.GetString("UserPhone"));
            HttpContext.Session.Clear();
            return RedirectToAction("LoginPage");

        }
        /*public IActionResult DisplaySumOfAddMoney()
        {
            string PhoneNumber = HttpContext.Session.GetString("UserPhone");
            transactionRepo transactionRepo1=new transactionRepo();
            decimal Sum=transactionRepo1.GetSumOfAddMoney(PhoneNumber);
            ViewBag.SumOfAddMoney= Sum;
            return View("Dashboard");
        }*/
        }
}