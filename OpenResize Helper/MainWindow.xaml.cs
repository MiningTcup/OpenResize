using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Win32.TaskScheduler;

namespace OpenResize_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void CreateSchedule(object sender, RoutedEventArgs e)
        {
            string programPath = Path.Combine(Directory.GetCurrentDirectory(), "OpenResize.exe");
            using TaskService taskService = new();
            if (!taskService.RootFolder.Tasks.Exists("StartOpenResize"))
            {
                TaskDefinition taskDefinition = taskService.NewTask();
                taskDefinition.RegistrationInfo.Description = "StartOpenResize";
                LogonTrigger logonTrigger = new();
                taskDefinition.Triggers.Add(logonTrigger);
                taskDefinition.Actions.Add(new ExecAction(programPath));
                taskService.RootFolder.RegisterTaskDefinition(
                    "StartOpenResize",
                    taskDefinition,
                    TaskCreation.CreateOrUpdate,
                    null,
                    null,
                    TaskLogonType.InteractiveToken,
                    null
                );

            }
        }

        private void RemoveSchedule(object sender, RoutedEventArgs e)
        {
            using TaskService taskService = new();
            if (taskService.RootFolder.Tasks.Exists("StartOpenResize")) taskService.RootFolder.DeleteTask("StartOpenResize");
        }
    }
}