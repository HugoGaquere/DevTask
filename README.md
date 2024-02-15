# DevTask

DevTask is a multiplatform project management tool designed to help developers keep track of their tasks within their codebase
It scans your project files for comments tagged with keywords (such as TODO, BUG, or REFACTOR)
and presents them in a user-friendly interface

![DevTask Screenshot](./devtask_screenshot.png)

## Features
- Scans your entire project directory for task comments.
- Displays tasks in a user-friendly interface.
- Sort tasks by type, file and date.
- Provides a summary of the number of tasks, files scanned, and scan time.
- Only support comments starting with '//' for now.
- [SOON] Supports different comment styles and file types.

## Development
This project is developed in C# and follows the Model-View-ViewModel (MVVM) architectural pattern.
It uses the [AvaloniaUI](https://avaloniaui.net/) framework for the user interface and [ReactiveUI](https://www.reactiveui.net/)
for reactive programming and facilitating the MVVM pattern.

## Roadmap
Here are some future improvements and features we're considering:

- Support for different comment styles and file types.
- Add tests.
- Add UI feedbacks during the scan process.
- Add multithreading to scan files in parallel for improved performance.
- Read and process files line by line or chunk by chunk to reduce memory consumption.
- User customization options, such as custom task tags and comment styles.
- Create an installer.
- Generate releases for Windows, Linux, and macOS.

Please note that this roadmap may change depending on project needs and contributions.

## Contributing
Contributions are welcome. Please open an issue to discuss your ideas or submit a pull request with your changes.  

## License
This project is licensed under the MIT License.