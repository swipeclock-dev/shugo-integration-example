using Newtonsoft.Json;

using Shugo.FileGuardian.Services.Common.Payroll;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Integration_Example
{
    internal class Program
    {
        public const string BASE_API_URL = "https://services.myfileguardian.com/";

        // Change these values to match your credentials.  We are using environment variables
        // here to avoid checking our credentials into a public repo, but as a user of the example
        // code, you can just hard-code your credentials here if you prefer.
        private static string PARTNER_API_KEY => Environment.GetEnvironmentVariable("SHUGO_EXAMPLE_API_KEY");
        private static string PARTNER_API_PASSWORD => Environment.GetEnvironmentVariable("SHUGO_EXAMPLE_API_PASS");

        private static PayrollBroker SHUGO_API;

        private static void Main(string[] args)
        {
            try
            {
                if (String.IsNullOrEmpty(PARTNER_API_KEY))
                {
                    Log("Please set the PARTNER_API_KEY property in order to run the example");
                    return;
                }

                if (String.IsNullOrEmpty(PARTNER_API_PASSWORD))
                {
                    Log("Please set the PARTNER_API_PASSWORD property in order to run the example");
                    return;
                }

                SHUGO_API = new PayrollBroker(BASE_API_URL, PARTNER_API_KEY, PARTNER_API_PASSWORD);

                WrapExampleCall(CreateCompany);
                WrapExampleCall(SetCompanyDepartment);
                WrapExampleCall(UpdateEmployees);
                WrapExampleCall(ProcessESSUpdates);
                WrapExampleCall(UploadPayrollSchedules);
                WrapExampleCall(ProcessPayrollData);
                WrapExampleCall(UploadCheckStubs);
            }
            catch (InvalidOperationException iex)
            {
                if (iex.Message != "API Error")
                {
                    Log(iex, $"Integration Example");
                }
            }
            catch (Exception ex)
            {
                Log(ex, $"Integration Example");
            }
            finally
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Log("Press enter to quit");
                    Console.ReadLine();
                }
            }
        }

        public const string ESS_NEW_HIRE = "AddNewHireToPayroll";
        public const string ESS_CHANGE_ADDRESS = "EmployeeHomeAddressUpdate";
        public const string ESS_CHANGE_PHONE = "EmployeeCellPhoneNumberUpdate";
        public const string ESS_CHANGE_TAX = "EmployeeTaxUpdate";
        public const string ESS_CHANGE_DEPOSIT = "EmployeeDirectDepositUpdate";

        // Company Codes and names must be unique, so generate a unique 
        // number here so we can run this example multiple times 
        // without having to change the company code/name every time
        private static string _uniq = null;
        public static string API_UNIQUIFIER
        {
            get
            {
                if (_uniq == null)
                {
                    _uniq = ((int)(DateTime.Now - DateTime.Now.Date).TotalSeconds).ToString();
                }

                return _uniq;
            }
        }

        private static string EXAMPLE_COMPANY_CODE => $"DTC{API_UNIQUIFIER}";
        private static string EXAMPLE_COMPANY_NAME => $"Delta Technology Controls{API_UNIQUIFIER}";
        private static int EXAMPLE_PROCESS_NUMBER = new Random().Next(100, 999);

        public static string EXAMPLE_ORG_GROUP_CODE_TEAM => "TEAM";
        public static string EXAMPLE_ORG_GROUP_CODE_REGION => "REG";
        public static string EXAMPLE_ORG_GROUP_CODE_PRODUCT => "PRODUCT";

        public class ExampleEmpData
        {
            public string EmployeeNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EmailAddress { get; set; }
            public string Password { get; set; }
            public Address Address { get; set; }
            public DateTime BirthDate { get; set; }
            public DateTime HireDate { get; set; }
            public bool AllowHUBLogin { get; set; }
            public bool EnableHUBFeatures { get; set; }
            public bool EmployeeTerminated { get; set; }
            public string SSN { get; set; }
            public string PhoneNumber { get; set; }
            public bool IsHUBAdmin { get; set; }
            public string CheckStubPDF { get; set; }
            public decimal GrossPayAmount { get; set; }
            public decimal NetPayAmount { get; set; }
            public string W2PDF { get; set; }
            public List<(string OrgGroupCode, string OrgItemCode)> OrgGroups { get; set; }
        }

        // In a real integration, this information would come from the Payroll System.  We will
        // create some example employees with this data
        public static List<ExampleEmpData> EXAMPLE_EMPLOYEE_DATA = new List<ExampleEmpData>
        {
            new ExampleEmpData {
                EmployeeNumber = "EMP042",
                FirstName = "Tara",
                LastName = "Thoris",
                EmailAddress = $"david.southern+{API_UNIQUIFIER}@gmail.com",
                Password = "Abcd1234!",
                Address = new Address()
                {
                    Address1 = "781 Sommerville Court",
                    City = "Richfield",
                    StateCode = "UT",
                    ZipCode = "84701"
                },
                BirthDate = DateTime.Parse("1987-12-19"),
                HireDate = DateTime.Parse("2019-05-13"),
                AllowHUBLogin = true,
                EnableHUBFeatures = true,
                EmployeeTerminated = false,
                SSN = "555-55-1234",
                PhoneNumber = "(512)555-8821",
                IsHUBAdmin = true,
                CheckStubPDF = "ExampleDocs/PS_042.pdf",
                GrossPayAmount = 3408.12M,
                NetPayAmount = 2106.21M,
                OrgGroups = new List<(string OrgGroupCode, string OrgItemCode)> {
                    (EXAMPLE_ORG_GROUP_CODE_TEAM, "4000"),
                    (EXAMPLE_ORG_GROUP_CODE_REGION, "03"),
                    (EXAMPLE_ORG_GROUP_CODE_PRODUCT, "PG-IOT"),
                }
            },
            new ExampleEmpData {
                EmployeeNumber = "EMP953",
                FirstName = "John",
                LastName = "Carter",
                EmailAddress = $"jcarter+{API_UNIQUIFIER}@example.com",
                Password = "Abcd1234!",
                Address = new Address {
                    Address1 = "128 State Street",
                    City = "Salt Lake City",
                    StateCode = "UT",
                    ZipCode = "84104"
                },
                BirthDate = DateTime.Parse("1992-11-30"),
                HireDate = DateTime.Parse("2019-05-21"),
                AllowHUBLogin = true,
                EnableHUBFeatures = true,
                EmployeeTerminated = false,
                SSN = "555-55-6543",
                PhoneNumber = "(512)555-5309",
                CheckStubPDF = "ExampleDocs/PS_953.pdf",
                GrossPayAmount = 2045.21M,
                NetPayAmount = 1396.87M,
                OrgGroups = new List<(string OrgGroupCode, string OrgItemCode)> {
                    (EXAMPLE_ORG_GROUP_CODE_TEAM, "2000"),
                    (EXAMPLE_ORG_GROUP_CODE_REGION, "01"),
                    (EXAMPLE_ORG_GROUP_CODE_PRODUCT, "PG-EMB"),
                }

            },
        };

        public static PayrollDataResponse CreateCompany()
        {
            Company newCompany = new Company
            {
                // CompanyCode must be a unique identifier for each company.  This typically
                // comes from the Payroll System, in order to make integration easier.
                CompanyCode = EXAMPLE_COMPANY_CODE,
                CompanyName = EXAMPLE_COMPANY_NAME,
                FederalEIN = "22-1111111",
                TimeZoneID = "Mountain Standard Time",
                PrimaryPhoneNumber = "(801)555-5309"
            };

            Log($"Creating company {newCompany.CompanyName}({newCompany.CompanyCode})");

            newCompany.PrimaryAddress = new Address
            {
                // Company address is necessary to help establish time zone 
                Address1 = "75 East Center",
                City = "Richfield",
                StateCode = "UT",
                ZipCode = "84701"
            };

            // Jurisdictions indicate the states that the client company has a relationship with.
            // In this case, the payroll customer hires employees in CA as well as UT, so we add
            // these jurisdictions to the client company.  Jurisdictions are used during onboarding 
            // to allow the hiring manager to indicate which forms the new hire needs to complete.
            newCompany.Jurisdictions.Add(
                new CoJurisdiction
                {
                    StateCode = "UT",
                    TaxID = "99887766"
                });

            newCompany.Jurisdictions.Add(
                new CoJurisdiction
                {
                    StateCode = "CA",
                    TaxID = "3334444"
                });

            // Select the processing mode of the client company (live vs onboarding)
            //
            //  Live = send emails and PUSH messages to employees
            //  Onboarding = do not send emails and PUSH messages to employees 
            //      while the company is being set up.
            newCompany.ProcessingMode = ProcessingModeType.Live;
            newCompany.Active = true;
            newCompany.DefaultPayFrequency = PayFrequencyType.BiWeekly;

            // Add the HUB feature to the company's FeatureList collection to enable HUB
            newCompany.FeatureList.Add(Feature.Hub);

            // Create (or update) the company.  We don't need to worry about whether the 
            // company already exists, the API will either create or update the company
            // appropriately.
            return SHUGO_API.AddOrUpdateCompany(newCompany);
        }

        public static PayrollDataResponse SetCompanyDepartment()
        {
            Company updateCompany = new Company
            {
                CompanyCode = EXAMPLE_COMPANY_CODE,
            };

            // HUB Defines a "Department" for every company that is used to organize
            // employees into groups for targeting Alerts, Workflows, etc.  We need to
            // select an Org Group from the external data source that we will map to the
            // HUB Department.  Indicate that this Org Group is the HUB Department by
            // setting the LevelNumber to 1
            CoOrgGroup hubDepartment = new CoOrgGroup()
            {
                OrgGroupCode = EXAMPLE_ORG_GROUP_CODE_TEAM,
                OrgGroupName = "Team",
                OrgType = OrgGroupType.Team,
                LevelNumber = 1
            };

            hubDepartment.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "1000",
                OrgItemName = "Admin"
            });

            hubDepartment.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "2000",
                OrgItemName = "Sales"
            });

            hubDepartment.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "3000",
                OrgItemName = "Marketing"
            });

            hubDepartment.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "4000",
                OrgItemName = "Development"
            });

            updateCompany.OrgGroups.Add(hubDepartment);


            // We can add additional Org Groups that can be used during Onboarding of
            // New Hires.  Each of these additional groups must have a unique Level 
            // Number greater than 1 (which indicates the HUB department) that 
            // indicates the heirarchy of the org groups.  In addition, there can
            // be only one Org Group of each OrgType that is not "Other".
            CoOrgGroup regionGroup = new CoOrgGroup()
            {
                OrgGroupCode = EXAMPLE_ORG_GROUP_CODE_REGION,
                OrgGroupName = "Region",
                OrgType = OrgGroupType.Other,
                LevelNumber = 2
            };

            regionGroup.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "01",
                OrgItemName = "Eastern Region"
            });

            regionGroup.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "02",
                OrgItemName = "Central Region"
            });

            regionGroup.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "03",
                OrgItemName = "Western Region"
            });

            updateCompany.OrgGroups.Add(regionGroup);


            CoOrgGroup teamGroup = new CoOrgGroup()
            {
                OrgGroupCode = EXAMPLE_ORG_GROUP_CODE_PRODUCT,
                OrgGroupName = "Product",
                OrgType = OrgGroupType.Other,
                LevelNumber = 3
            };

            teamGroup.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "PG-EMB",
                OrgItemName = "Embedded Devices"
            });

            teamGroup.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "PG-IOT",
                OrgItemName = "IoT Controls"
            });

            teamGroup.OrgItems.Add(new CoOrgItem()
            {
                OrgItemCode = "PG-PLC",
                OrgItemName = "PLC/DCS/PID"
            });

            updateCompany.OrgGroups.Add(teamGroup);

            return SHUGO_API.AddOrUpdateCoOrgGroupsAndItems(updateCompany);
        }

        public static PayrollDataResponse UpdateEmployees()
        {
            Company updateCompany = new Company
            {
                CompanyCode = EXAMPLE_COMPANY_CODE
            };

            // Notes on Adding Employees
            //
            // You must include an addess with a Zip code, the SSNLastFour, and a BirthDate
            // in order to activate employee and auto send an invite.
            //
            // Password field(Optional) - If defined this will be the employee's first time password 
            // for logging into HUB. If set to null, HUB will assign its own default.
            // The password can only be set at the time the employee is first added to HUB.
            // If a password is set on a call to update the employee it will be ignored.

            foreach (ExampleEmpData empData in EXAMPLE_EMPLOYEE_DATA)
            {
                Employee uploadEmp = new Employee
                {
                    EmployeeNumber = empData.EmployeeNumber,
                    FirstName = empData.FirstName,
                    LastName = empData.LastName,
                    EmailAddress = empData.EmailAddress,
                    Password = empData.Password,
                    PrimaryAddress = empData.Address,
                    BirthDate = empData.BirthDate,
                    SSNFull = empData.SSN,
                    CellPhoneNumber = empData.PhoneNumber,
                    HireDate = empData.HireDate.ToString("yyyy-MM-dd"),
                };

                // In order for HUB to automatically send activation emails, the employee
                // must be created with an Email, Zip Code, BirthDate and SSNLastFour set.
                // Even though we are setting SSNFull, we still have to explicitly set SSNLastFour
                // here in order for the Activation email to be sent.
                if (!String.IsNullOrEmpty(uploadEmp.SSNFull) && uploadEmp.SSNFull.Length >= 4)
                {
                    uploadEmp.SSNLastFour = uploadEmp.SSNFull.Substring(uploadEmp.SSNFull.Length - 4);
                }

                // Note: The difference between Employee.Status and Employee.Active:
                //    Employee.Status == Terminated means that the employee is no longer employed
                //    so HUB features will be disabled, however the employee account is still 
                //    enabled so that the terminated employee can log in to receive termination
                //    document, like last pay stub, W2s, etc.  Note that clients can still be
                //    billed for a Terminated employee if any documents are delivered
                //
                //    Employee.Active == false means that the employee account is fully
                //    disabled, and the employee will no longer be able to receive any documents
                //    or HUB features.  Setting the employee Active to false means that the
                //    client will not be billed for that account.

                uploadEmp.Status = empData.EmployeeTerminated
                    ? EmploymentStatus.Terminated : EmploymentStatus.Active;

                // If Employee.Active is false then the employee is not allowed to log
                // into hub at all.
                uploadEmp.Active = empData.AllowHUBLogin;

                if (empData.EnableHUBFeatures || empData.IsHUBAdmin)
                {
                    // Any employee that will use HUB functionality needs to be indicated by
                    // adding the Feature.HubBasic flag to that employee's FeatureList
                    uploadEmp.FeatureList.Add(Feature.HubBasic);
                }

                // If the integration knows which employees are managers/supervisors, it
                // can indicate this by making these employees HUB administrators by adding
                // the Feature.HubAdministrator flag
                if (empData.IsHUBAdmin)
                {
                    uploadEmp.FeatureList.Add(Feature.HubAdministrator);
                }

                // Set the employee's Org Group memberships
                foreach ((string OrgGroupCode, string OrgItemCode) in empData.OrgGroups)
                {
                    EeHomeOrgItem orgItem = new EeHomeOrgItem
                    {
                        OrgGroupCode = OrgGroupCode,
                        OrgItemCode = OrgItemCode
                    };

                    uploadEmp.HomeOrgItems.Add(orgItem);
                }

                updateCompany.Employees.Add(uploadEmp);
                Log($"Adding Employee: {uploadEmp.EmailAddress}");
            }

            return SHUGO_API.AddOrUpdateEmployees(updateCompany);
        }

        public static PayrollDataResponse RoundTripNewHire(NewHire nh)
        {
            // API EXAMPLE WARNING: In a payroll system integration, you would not 
            // round-trip the NewHire directly as part of the ESS New Hire 
            // processing.  Typically you would add the New Hire to the external
            // Payroll System, and then later when the regular Employee Sync ran, 
            // the new hire would be round-tripped with all of the appropriate 
            // Payroll System information.  For the purposes of this example,
            // we will round-trip the employee here so we can see the results
            // in our example client.

            Company updateCompany = new Company
            {
                CompanyCode = EXAMPLE_COMPANY_CODE
            };

            DateTime birthDate = Convert.ToDateTime(nh.BirthDate);

            string EXAMPLE_NEWHIRE_EMPCODE = "EMP675";

            Employee nhEmp = new Employee
            {
                // We need to add an Employee Code, as that field is created
                // by the Payroll System, so just make up a random emp code 
                // here.  All of the rest of the data will be provided by HUB 
                // as part of the New Hire process
                EmployeeNumber = EXAMPLE_NEWHIRE_EMPCODE,
                // Integrators must be sure to include the NewHireID that was received from the
                // GetPendingChanges API call when creating the new employee.  WorkforceHUB uses
                // the NewHireID to link incoming Employees with the NewHire/Onboarding data and
                // documentation that was collected during the WFH Onboarding workflow.  In this
                // example, we still have the NewHire data available from the GetPendingChanges
                // call, but normally the Employee round-trip would be completed later by the
                // standard employee sync process.  In that case, the integrator should be sure
                // to store the NewHireID in a way that it can be retrieved when syncing the
                // employee later.
                NewHireID = nh.NewHireID,
                FirstName = nh.LegalFirstName,
                LastName = nh.LegalLastName,
                EmailAddress = nh.EmailAddress,
                Password = "Abcd1234!",
                BirthDate = birthDate,
                Status = EmploymentStatus.Active,
                CellPhoneNumber = nh.CellPhoneNumber,
                HireDate = nh.HireDate,
                SSNFull = nh.SSN
            };

            nhEmp.PrimaryAddress = new Address()
            {
                Address1 = nh.HomeAddress.Address1,
                City = nh.HomeAddress.City,
                StateCode = nh.HomeAddress.StateCode,
                ZipCode = nh.HomeAddress.ZipCode
            };

            // Set up with HUB access
            nhEmp.FeatureList.Add(Feature.HubBasic);

            updateCompany.Employees.Add(nhEmp);

            return SHUGO_API.AddOrUpdateEmployees(updateCompany);
        }

        public static PayrollDataResponse ProcessESSUpdates()
        {
            // Note: In order to see any activity in this example API call, you will have
            // to make some changes in HUB.  If you log in as an employee and change address,
            // phone, or tax information, you will see the appropriate ESS updates.  If you log
            // in as an admin and complete a New Hire workflow, you will see the New Hire update.

            List<PendingChange> essChanges = SHUGO_API.GetPendingChanges(null);

            foreach (PendingChange nextChange in essChanges)
            {
                string companyCode = nextChange.Parameters.GetValueByKey("CompanyCode");
                string empNumber = null;
                Employee empData = null;

                if (nextChange.ChangeType != ESS_NEW_HIRE)
                {
                    empNumber = nextChange.Parameters.GetValueByKey("EmployeeNumber");
                    empData = SHUGO_API.GetEmployee(null, companyCode, empNumber);
                }

                switch (nextChange.ChangeType)
                {
                    case ESS_NEW_HIRE:
                        string NewHireID = nextChange.Parameters.GetValueByKey("NewHireID");
                        NewHire newEmp = SHUGO_API.GetNewHire(null, companyCode, NewHireID);
                        Log($"Send NewHire data to Payroll System:");
                        Log($"    {newEmp.LegalLastName}, {newEmp.LegalFirstName}/{newEmp.EmailAddress}/{newEmp.HireDate}/{newEmp.NewHireID}");

                        // API EXAMPLE WARNING: In a payroll system integration, you would not 
                        // round-trip the NewHire directly as part of the ESS New Hire 
                        // processing.  Typically you would add the New Hire to the external
                        // Payroll System, and then later when the regular Employee Sync ran, 
                        // the new hire would be round-tripped with all of the appropriate 
                        // Payroll System information.  For the purposes of this example,
                        // we will round-trip the employee here so we can see the results
                        // in our example client.
                        RoundTripNewHire(newEmp);
                        break;

                    case ESS_CHANGE_ADDRESS:
                        Log($"Update emp address on Payroll system with: {empData.EmployeeNumber} - {empData.PrimaryAddress.Address1}/{empData.PrimaryAddress.City}/{empData.PrimaryAddress.StateCode}/{empData.PrimaryAddress.ZipCode}");
                        break;

                    case ESS_CHANGE_PHONE:
                        Log($"Update emp phone on Payroll system with: {empData.EmployeeNumber} - {empData.CellPhoneNumber}");
                        break;

                    case ESS_CHANGE_DEPOSIT:
                        List<DirectDepositRule> eeDirDeps = SHUGO_API.GetEmployeeDirectDeposits(null, companyCode, empNumber);

                        Log($"Update emp deduction on Payroll system with: {empData.EmployeeNumber}");

                        foreach (DirectDepositRule eeDD in eeDirDeps)
                        {
                            Log($"    Bank: {eeDD.BankName}, Rte: {eeDD.BankRoutingNumber}, Acct: {eeDD.BankAccountNumber}, %: {eeDD.PercentOfNetPay}");
                        }
                        break;

                    case ESS_CHANGE_TAX:
                        int taxId = Convert.ToInt32(nextChange.Parameters.GetValueByKey("TaxID"));
                        EeTax eeTax = SHUGO_API.GetEmployeeTax(null, companyCode, empNumber, taxId);

                        Log($"Update emp tax on Payroll system with: {empData.EmployeeNumber} - {empData.LastName}, {empData.FirstName} - {eeTax.TaxName}/{eeTax.MaritalStatusCode}/{eeTax.JurisdictionLevel}/{eeTax.AllowanceNumber}/{eeTax.AdditionalWithholdingPerPay}");
                        break;
                }

                // If we do not acknowledge the pending change, we will continue to get
                // it each time we call GetPendingChanges()
                SHUGO_API.AcknowledgePendingChange(nextChange.ChangeID);
            }

            return new PayrollDataResponse { IsSuccessful = true };
        }

        public static PayrollDataResponse UploadPayrollSchedules()
        {
            Company updateCompany = new Company
            {
                CompanyCode = EXAMPLE_COMPANY_CODE
            };

            // Populate the next two upcoming scheduled payrolls
            DateTime curPeriodEndDate = GetNextDayOfWeek(DayOfWeek.Saturday);

            Payroll curScheduledPayroll = new Payroll()
            {
                PeriodStartDate = curPeriodEndDate.AddDays(-13),
                PeriodEndDate = curPeriodEndDate,
                InputDate = curPeriodEndDate.AddDays(3),
                CheckDate = curPeriodEndDate.AddDays(6),
                Status = PayrollStatus.Scheduled
            };

            DateTime nextPeriodEndDate = curPeriodEndDate.AddDays(14);

            Payroll nextScheduledPayroll = new Payroll()
            {
                PeriodStartDate = nextPeriodEndDate.AddDays(-13),
                PeriodEndDate = nextPeriodEndDate,
                InputDate = nextPeriodEndDate.AddDays(3),
                CheckDate = nextPeriodEndDate.AddDays(6),
                Status = PayrollStatus.Scheduled
            };

            // Attach both payrolls to the company
            updateCompany.Payrolls.Add(curScheduledPayroll);
            updateCompany.Payrolls.Add(nextScheduledPayroll);

            // Attach the company to the request.
            //  Note: The ProcessPayrollData() method can handle batches of multiple companies' payrolls.
            PayrollDataRequest payrollScheduleRequest = new PayrollDataRequest
            {
                Companies = new List<Company> { updateCompany }
            };

            return SHUGO_API.ProcessPayrollData(payrollScheduleRequest);
        }

        public static PayrollDataResponse ProcessPayrollData()
        {
            // When a payroll is processed in the payroll system, a record of that payroll should 
            // be sent to HUB.  This is used by HUB to issue payroll reminders and alerts
            //
            // The following information should be supplied:
            //      - Cash Required for the payroll
            //      - Amount debited from the payroll customer's bank account (if necessary)
            //      - Net pay amounts for each employee check
            //      - Direct deposits issued for each net pay
            //      - The next two scheduled payrolls for the company

            Company updateCompany = new Company
            {
                CompanyCode = EXAMPLE_COMPANY_CODE
            };

            PayrollDataRequest processingRequest = new PayrollDataRequest
            {
                Companies = new List<Company> { updateCompany }
            };

            // Payroll Object Notes
            //
            //  - CheckDate: 
            //    The check date of the payroll
            //  - ProcessNumber:
            //    The run number assigned to this payroll by the payroll software.  Note that HUB uses 
            //    the combination of check date and process number uniquely identify payrolls.  
            //  - Status:
            //    Indicates whether the payroll was processed or scheduled in the payroll system.
            //  - CashRequirementAmount: 
            //    The total cash required by the payroll customer for the payroll.
            //  - AchDebitAmount: (Optional)
            //    The total amount to be debited from the customer's bank account on or before check 
            //    date.  This is typically an equal or smaller number than the cash requirement amount.  
            //    If the payroll customer is not having their bank account debited, this should be null.
            //  - AchDebitDate: (Optional)
            //    If money is being debited from the customer's bank account, this is the date that 
            //    the debit will take place.

            // Get the current payroll end date
            DateTime currentPayrollEndDate = GetNextDayOfWeek(DayOfWeek.Saturday).AddDays(-14);

            // In our example Payroll System, check stubs come out 6 days after the pay period ends...
            DateTime currentPayrollCheckDate = currentPayrollEndDate.AddDays(6);

            string payrollTag = $"{currentPayrollCheckDate.ToString("yyyy-MM-dd")}/{EXAMPLE_PROCESS_NUMBER}";

            Log($"Creating Payroll data for payroll {payrollTag}");

            Payroll processedPayroll = new Payroll
            {
                PeriodStartDate = currentPayrollEndDate.AddDays(-13),
                PeriodEndDate = currentPayrollEndDate,
                InputDate = currentPayrollEndDate.AddDays(3),
                CheckDate = currentPayrollCheckDate,
                ProcessNumber = EXAMPLE_PROCESS_NUMBER,
                Status = PayrollStatus.Processed,
                AchDebitDate = currentPayrollCheckDate.AddDays(-2)
            };

            // Populate the net pays that were issued as part of this payroll 
            // (both printed checks and direct deposited net pays)
            AppendNetPaysToPayroll(processedPayroll);

            // Add the processed payroll to the company
            updateCompany.Payrolls.Add(processedPayroll);

            // Populate the next two upcoming scheduled payrolls
            DateTime nextPeriodEndDate = currentPayrollEndDate.AddDays(14);

            Payroll nextScheduledPayroll = new Payroll()
            {
                PeriodStartDate = nextPeriodEndDate.AddDays(-13),
                PeriodEndDate = nextPeriodEndDate,
                InputDate = nextPeriodEndDate.AddDays(3),
                CheckDate = nextPeriodEndDate.AddDays(6),
                Status = PayrollStatus.Scheduled
            };

            updateCompany.Payrolls.Add(nextScheduledPayroll);

            nextPeriodEndDate = nextPeriodEndDate.AddDays(14);

            nextScheduledPayroll = new Payroll()
            {
                PeriodStartDate = nextPeriodEndDate.AddDays(-13),
                PeriodEndDate = nextPeriodEndDate,
                InputDate = nextPeriodEndDate.AddDays(3),
                CheckDate = nextPeriodEndDate.AddDays(6),
                Status = PayrollStatus.Scheduled
            };

            updateCompany.Payrolls.Add(nextScheduledPayroll);

            return SHUGO_API.ProcessPayrollData(processingRequest);
        }

        private static void AppendNetPaysToPayroll(Payroll processedPayroll)
        {
            decimal totalPayrollAmount = 0;
            decimal totalACHAmount = 0;

            // Populate the net pays that were issued as part of this payroll 
            // (both printed checks and direct deposited net pays)

            // Note: if the employee wants a printed check with no direct deposit
            // the Distributions collection should be left empty.
            EmployeeCheck printedCheck = new EmployeeCheck
            {
                EmployeeNumber = EXAMPLE_EMPLOYEE_DATA[0].EmployeeNumber,
                NetPayAmount = EXAMPLE_EMPLOYEE_DATA[0].NetPayAmount
            };

            totalPayrollAmount += EXAMPLE_EMPLOYEE_DATA[0].GrossPayAmount;
            totalACHAmount += EXAMPLE_EMPLOYEE_DATA[0].NetPayAmount;

            processedPayroll.Checks.Add(printedCheck);

            // For an employee that prefers direct deposit:
            // In this case, we will show how to handle a check that 
            // is deposited into two bank accounts
            decimal totalCheckAmount = EXAMPLE_EMPLOYEE_DATA[1].NetPayAmount;
            decimal firstBankAmount = totalCheckAmount / 3;
            decimal secondBankAmount = totalCheckAmount = firstBankAmount;

            EmployeeCheck directDepositedCheck = new EmployeeCheck()
            {
                EmployeeNumber = EXAMPLE_EMPLOYEE_DATA[1].EmployeeNumber,
                NetPayAmount = totalCheckAmount
            };

            totalPayrollAmount += EXAMPLE_EMPLOYEE_DATA[1].GrossPayAmount;
            totalACHAmount += EXAMPLE_EMPLOYEE_DATA[1].NetPayAmount;

            // Note: HUB only uses the last 3 digits of the bank account number.
            // For the integration, either the entire bank account number or only the 
            // last three digits of it can be supplied.
            directDepositedCheck.Distributions.Add(new NetPayDistribution
            {
                DistributionName = "Checking",
                BankAccountNumber = "8675309",
                Amount = firstBankAmount
            });

            directDepositedCheck.Distributions.Add(new NetPayDistribution
            {
                DistributionName = "Savings",
                BankAccountNumber = "8674221",
                Amount = secondBankAmount
            });

            processedPayroll.Checks.Add(directDepositedCheck);

            processedPayroll.CashRequirementAmount = totalPayrollAmount;
            processedPayroll.AchDebitAmount = totalACHAmount;
        }

        public static PayrollDataResponse UploadCheckStubs()
        {
            // Begin by creating a Payroll Staging file to hold the uploaded payroll document images
            PayrollDataResponse _stageFileResult = SHUGO_API.StagePayrollFile(null, EXAMPLE_COMPANY_CODE);

            if (!_stageFileResult.IsSuccessful)
            {
                Log("Creating Payroll Stage file was unsuccessful");
                return _stageFileResult;
            }

            // Get the previous payroll end date
            DateTime checkStubDate = GetNextDayOfWeek(DayOfWeek.Saturday).AddDays(-14);

            // In our example Payroll System, check stubs come out 6 days after the pay period ends...
            checkStubDate = checkStubDate.AddDays(6);

            string payrollTag = $"{checkStubDate.ToString("yyyy-MM-dd")}/{EXAMPLE_PROCESS_NUMBER}";

            Log($"Upload Payroll docs for payroll {payrollTag}");

            int totalDocCount = 0;

            foreach (ExampleEmpData empData in EXAMPLE_EMPLOYEE_DATA)
            {
                if (!String.IsNullOrEmpty(empData.CheckStubPDF))
                {
                    using (FileStream _fileContents = new FileStream(empData.CheckStubPDF, FileMode.Open))
                    {
                        PayrollFile checkStubFile = new PayrollFile()
                        {
                            RequestID = _stageFileResult.RequestID,
                            CompanyCode = EXAMPLE_COMPANY_CODE,
                            EventDate = checkStubDate,
                            ProcessNumber = EXAMPLE_PROCESS_NUMBER,
                            Amount = empData.NetPayAmount,
                            EmployeeNumber = empData.EmployeeNumber,
                            // You can also upload W2s, 1099s, and 1095s with this call
                            FileType = PayrollFileType.EmployeeCheckStub,
                            FileContents = _fileContents
                        };

                        PayrollFileResponse _checkStubUploadResult = SHUGO_API.AppendToPayrollFile(checkStubFile);

                        if (!_checkStubUploadResult.IsSuccessful)
                        {
                            return new PayrollDataResponse
                            {
                                IsSuccessful = false,
                                Message = $"Uploading Check stub file '{empData.CheckStubPDF}' was unsuccessful"
                            };
                        }

                        totalDocCount++;
                    }
                }
            }

            // Complete the creation of the payroll file
            PayrollDataResponse _completeFileResult =
                SHUGO_API.CompletePayrollFile(_stageFileResult.RequestID, totalDocCount);

            if (_completeFileResult.IsSuccessful == false)
            {
                Log("Completion of Payroll file was unsuccessful");
            }

            return _completeFileResult;
        }

        // Utility methods

        public static void Log(string message = null)
        {
            if (String.IsNullOrEmpty(message))
            {
                Console.WriteLine();
                return;
            }

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}: {message}");
        }

        public static void Log(Exception ex, string message = null)
        {
            if (ex == null)
            {
                Log(message);
                return;
            }

            do
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}: {message} - Exception: {ex.GetType().Name} - {ex.Message}");
                ex = ex.InnerException;
            } while (ex != null);
        }

        private static void CheckAPIResponse<T>(string description, T response) where T : class
        {
            if (response == null)
            {
                Log($"{description} - No response returned");
                return;
            }

            PayrollDataResponse payrollResponse = response as PayrollDataResponse;

            if (payrollResponse != null)
            {
                if (payrollResponse.IsSuccessful)
                {
                    Log($"{description} Successful");
                    return;
                }

                Log($"{description} Payroll API Error: {payrollResponse.Message}");

                if (payrollResponse.BrokenRules.Count > 0)
                {
                    Log("Broken Rules: ");

                    foreach (BrokenRule rule in payrollResponse.BrokenRules)
                    {
                        Log($"    {rule.BrokenRuleCode}({rule.EntityUniqueKey}): {rule.Message}");
                    }
                }

                throw new ArgumentException($"{description}: API Call Failed");
            }

            throw new ArgumentException("CheckAPIResponse: response is not an API response type");
        }

        private static void WrapExampleCall<T>(Func<T> exampleMethod, string description = null) where T : class
        {
            if (String.IsNullOrEmpty(description))
            {
                description = exampleMethod.GetMethodInfo()?.Name;
            }

            Log();
            Log();
            Log($"Starting {description}...");

            CheckAPIResponse(description, exampleMethod());
        }

        private static DateTime GetNextDayOfWeek(DayOfWeek dayOfWeek)
        {
            return GetNextDayOfWeek(DateTime.Today, dayOfWeek);
        }

        private static DateTime GetNextDayOfWeek(DateTime asOfDate, DayOfWeek dayOfWeek)
        {
            if (asOfDate.DayOfWeek > dayOfWeek)
            {
                return asOfDate.AddDays((int)asOfDate.DayOfWeek).Date;
            }
            else
            {
                return asOfDate.AddDays(dayOfWeek - asOfDate.DayOfWeek).Date;
            }
        }
    }
}
