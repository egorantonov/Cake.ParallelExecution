///////////////////////
// Import
///////////////////////
#addin nuget:?package=Cake.Powershell

///////////////////////
// Variables
///////////////////////
var taskBodyFormat = "Get-ChildItem -Path '{0}' -Recurse -ErrorAction SilentlyContinue | ForEach-Object {{ Get-FileHash -Path $_.FullName -Algorithm SHA512 -ErrorAction SilentlyContinue }}";
var parallelInvokeFormat = "start powershell {{.\\build.ps1 -Target '{0}'; echo 'Parallel task execution finished.'; Read-Host}}";

///////////////////////
// Tasks and wrappers
///////////////////////

Task("First-Task")
    .Does(() => {
        StartPowershellScript(string.Format(taskBodyFormat, "C:\\Windows\\SysWOW64"));
    });

Task("Second-Task")
    .Does(() => {
        StartPowershellScript(string.Format(taskBodyFormat, "C:\\Program Files (x86)\\Microsoft"));
    });

Task("Third-Task")
    .Does(() => {
        StartPowershellScript(string.Format(taskBodyFormat, "C:\\Windows\\Installer"));
    });

// a long-running task example
Task("Fourth-Task")
    .Does(() => {
        StartPowershellScript(string.Format(taskBodyFormat, "C:\\Windows\\System32"));
    });

Task("Init-Parallel-First-Task")
    .Does(() => {
        StartPowershellScript(string.Format(parallelInvokeFormat, "First-Task"));
    });

Task("Init-Parallel-Second-Task")
    .Does(() => {
        StartPowershellScript(string.Format(parallelInvokeFormat, "Second-Task"));
    });

Task("Init-Parallel-Third-Task")
    .Does(() => {
        StartPowershellScript(string.Format(parallelInvokeFormat, "Third-Task"));
    });

// a scope of tasks, which aren't long-running
Task("LightScope")
    .IsDependentOn("First-Task")
    .IsDependentOn("Second-Task")
    .IsDependentOn("Third-Task");

// wrapper for scoped task
Task("Init-Parallel-Scoped-Task")
    .Does(() => {
        StartPowershellScript(string.Format(parallelInvokeFormat, "LightScope"));
    });

///////////////////////
// Execution
///////////////////////

// an example of typical sequential tasks
// Execution time: 01:56.4556927
Task("Sequential")
    .IsDependentOn("First-Task")
    .IsDependentOn("Second-Task")
    .IsDependentOn("Third-Task")
    .IsDependentOn("Fourth-Task");

// an example of full parallelization with 4 Powershell instances
// Execution time: 01:35.8328450
Task("Parallel")
    .IsDependentOn("Init-Parallel-First-Task")
    .IsDependentOn("Init-Parallel-Second-Task")
    .IsDependentOn("Init-Parallel-Third-Task")
    .IsDependentOn("Fourth-Task");

// an example with 2 instances of Powershell
// Execution time: 01:24.0900724
Task("Parallel-Scoped")
    .IsDependentOn("Init-Parallel-Scoped-Task")
    .IsDependentOn("Fourth-Task");

// a good example of sequential tasks
// Execution time: 00:21.8349129
Task("Sequential-Better")
    .IsDependentOn("Second-Task")
    .IsDependentOn("First-Task");

// a bad example of parallel tasks, second task Cake build time added (~ 10-12 s)
// Execution time: 00:24.1812227
Task("Parallel-Worse")
    .IsDependentOn("Init-Parallel-First-Task")
    .IsDependentOn("Second-Task");