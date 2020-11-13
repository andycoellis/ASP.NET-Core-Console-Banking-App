#### Summary
This repository is a prototype console-based banking system built C# using ADO.NET and an SQL server backend. This project was built for Assignment 1 of Web Development Technologies.

The system features:
> Login with hashed passwords and stored accounts
<br>Account features (update details, open accounts, display summaries)
<br>Transaction features (deposit, withdrawal, transfers)

*Additional features*
> Customer Sign Up
<br>Update Customer details
<br>Reset user password
<br>Password attempts at login -> reset

#### Design Patterns
**Facade/Factory**
<br>We have implemented a facade class (ControllerFacade.cs) to hide the underlying functionality of our controllers. We wanted to prevent the user interface classes from receiving any information about our database or methods. The AppEngine, passed to any user interface (eg. Console Calback), handles method calls through the facade. This has the advantage of letting us hide all information about underlying functions, and simplify method usage for the UI. Our code is more elegant as all functions called are simple one line calls, which shows all the relevant functions.

<br>Additionally the Facade also utilised a Factory pattern, such that all Controllers behind the Facade implemented the Interface IController, this allowed more CRUD attributes enforced and utiliesed around our controllers. It not used methods would be to unique and behaviour may not have been constant across the controllers.

**MVC**
<br>Our project uses a simple MVC structure. Our models handle all data storage and manipulation, our controllers handle database and interface updates, and our view only displays UI output. We have used these patterns to attempt to make our code as compartmental as possible. This has the advantage of increasing our code reusability and extensibility, as our view, for example, could be replaced by a completely different view without breaking the project. This helps our code to be more elegant as each section of the MVC contains similar functions and features, making the code easier to learn and integrate.

<Additionally having an appropriate file structure to the project helps improve namespace understanding for programmer quick reference.>
