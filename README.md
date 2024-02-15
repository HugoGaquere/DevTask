# DevTask

DevTask is a project management tool designed to help developers keep track of their tasks within their codebase
It scans your project files for comments tagged with keywords (such as TODO, BUG, or REFACTOR)
and presents them in a user-friendly interface

## Features
- Scans your entire project directory for task comments;
- Displays tasks in a user-friendly interface;
- Provides a summary of the number of tasks, files scanned, and scan time;
- Only support comments starting with '//' for now;
- [SOON] Supports different comment styles and file types.

## Development
This project is developed in C# and follows the Model-View-ViewModel (MVVM) architectural pattern.
It uses the [AvaloniaUI](https://avaloniaui.net/) framework for the user interface and [ReactiveUI](https://www.reactiveui.net/)
for reactive programming and facilitating the MVVM pattern.

## Contributing
Contributions are welcome. Please open an issue to discuss your ideas or submit a pull request with your changes.  


## License
This project is licensed under the MIT License.