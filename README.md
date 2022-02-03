# PasswordGen

PasswordGen is a UWP application that generates passwords offline.

To install it, visit [Microsoft Store](https://www.microsoft.com/store/apps/9N41PKLFWJNJ).

UI testing is done with [WinAppDriver](https://github.com/microsoft/WinAppDriver) and MSTest.

Details about the way passwords are generated:
- A new password is automatically generated at startup.
- Only ASCII characters are used, except control characters and the space.
- On the home page, the number of password characters from a specific charset (e.g. digits) is at least one when the corresponding switch is on and zero otherwise.

**Credits**  
Software made with [Visual Studio](https://visualstudio.microsoft.com)  
Icon made by [bqlqn](https://www.flaticon.com/authors/bqlqn) from [www.flaticon.com](https://www.flaticon.com)

**Copyright**  
© 2021-2022 Johnny A. F. Gérard
