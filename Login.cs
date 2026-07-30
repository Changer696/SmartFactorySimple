using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartFactorySimple;

public class Login
{
    private readonly string CREDENTIALS_FILE;
    private readonly Dictionary<string, EmployeeCredential> credentials = new Dictionary<string, EmployeeCredential>();

    public class EmployeeCredential
    {
        public string EmployeeId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public Login()
    {
        // Set credentials file path in the project directory
        CREDENTIALS_FILE = AppFileNames.ResolvePath(AppFileNames.EmployeesFileName);
        LoadCredentials();
    }

   
    private void LoadCredentials()
    {
        try
        {
            if (!File.Exists(CREDENTIALS_FILE))
            {
                Console.WriteLine(Messages.MissingFile(CREDENTIALS_FILE));
                return;
            }

            credentials.Clear();
            string[] lines = File.ReadAllLines(CREDENTIALS_FILE);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                string[] parts = line.Split(';');
                if (parts.Length != 4)
                {
                    Console.WriteLine(Messages.InvalidLineFormat(line));
                    continue;
                }

                var credential = new EmployeeCredential
                {
                    EmployeeId = parts[0].Trim(),
                    Username = parts[1].Trim(),
                    Password = parts[2].Trim(),
                    Role = parts[3].Trim()
                };

                credentials[credential.Username] = credential;
            }

            Console.WriteLine(Messages.LoadedCredentials(credentials.Count));
        }
        catch (Exception ex)
        {
            Console.WriteLine(Messages.LoadingCredentialsError(ex.Message));
        }
    }

    
    public EmployeeCredential Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        if (credentials.ContainsKey(username))
        {
            EmployeeCredential credential = credentials[username];
            if (credential.Password == password)
            {
                return credential;
            }
        }

        return null;
    }

    
    public EmployeeCredential PromptLogin()
    {
        Console.WriteLine(Messages.LoginHeader);
        Console.Write(Messages.UsernamePrompt);
        string username = Console.ReadLine();

        Console.Write(Messages.PasswordPrompt);
        string password = ReadPassword();

        EmployeeCredential credential = Authenticate(username, password);

        if (credential != null)
        {
            Console.WriteLine(Messages.WelcomeUser(username, credential.Role));
            Logging.Log(credential.Username, "Successful login");
            return credential;
        }
        else
        {
            Console.WriteLine(Messages.InvalidCredentials);
            var userForLog = string.IsNullOrWhiteSpace(username) ? "unknown" : username;
            Logging.Log(userForLog, "Failed login attempt");
            return null;
        }
    }

    
    public EmployeeCredential LoginWithAttempts(int maxAttempts = 3)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            EmployeeCredential credential = PromptLogin();
            if (credential != null)
            {
                return credential;
            }

            if (attempt < maxAttempts)
            {
                Console.WriteLine(Messages.AttemptFailed(attempt, maxAttempts - attempt));
            }
        }

        Console.WriteLine(Messages.LoginFailedMaxAttempts);
        return null;
    }

   
    public bool SaveEmployeeCredential(string employeeId, string username, string password, string role)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(employeeId) || string.IsNullOrWhiteSpace(username) || 
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                Console.WriteLine(Messages.MissingCredentialFields);
                return false;
            }

            if (credentials.ContainsKey(username))
            {
                Console.WriteLine(Messages.UsernameAlreadyExists(username));
                return false;
            }

            string credentialLine = $"{employeeId};{username};{password};{role}";
            
            // Append to file
            File.AppendAllText(CREDENTIALS_FILE, credentialLine + Environment.NewLine);
            
            // Update in-memory dictionary
            var credential = new EmployeeCredential
            {
                EmployeeId = employeeId,
                Username = username,
                Password = password,
                Role = role
            };
            credentials[username] = credential;

            Console.WriteLine(Messages.CredentialsSaved(username, role));
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(Messages.SaveCredentialsError(ex.Message));
            return false;
        }
    }

    public EmployeeCredential FindCredentialByEmployeeId(string employeeId)
    {
        return credentials.Values.FirstOrDefault(c => c.EmployeeId == employeeId);
    }

    public bool RemoveEmployeeCredentialByEmployeeId(string employeeId)
    {
        var credential = FindCredentialByEmployeeId(employeeId);
        if (credential == null)
            return false;

        credentials.Remove(credential.Username);
        return SaveAllCredentials();
    }

    public bool AddEmployeeCredential(EmployeeCredential credential)
    {
        if (credential == null)
            return false;

        if (credentials.ContainsKey(credential.Username))
            return false;

        string credentialLine = $"{credential.EmployeeId};{credential.Username};{credential.Password};{credential.Role}";
        File.AppendAllText(CREDENTIALS_FILE, credentialLine + Environment.NewLine);
        credentials[credential.Username] = credential;
        return true;
    }

    private bool SaveAllCredentials()
    {
        try
        {
            var lines = credentials.Values.Select(c => $"{c.EmployeeId};{c.Username};{c.Password};{c.Role}");
            File.WriteAllLines(CREDENTIALS_FILE, lines);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(Messages.SaveCredentialsError(ex.Message));
            return false;
        }
    }

    // Read password from console while masking input with '*'
    public string ReadPassword()
    {
        var pwd = new System.Text.StringBuilder();
        ConsoleKeyInfo key;
        while (true)
        {
            key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (pwd.Length > 0)
                {
                    pwd.Length--;
                    // move cursor back, write space, move back again
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                pwd.Append(key.KeyChar);
                Console.Write("*");
            }
        }

        return pwd.ToString();
    }
}
