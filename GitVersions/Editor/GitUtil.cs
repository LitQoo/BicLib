using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug; 

public static class GitUtil
{
    [MenuItem("BicLib/GitTest")]
    public static void LogCommit()
    {
        var result = Command("status");
        Debug.Log(result);
    }
    
    public static string Command(string gitCommand)
    {
        // Strings that will catch the output from our process.
        string output = "no-git";
        string errorOutput = "no-git";

        // Set up our processInfo to run the git command and log to output and errorOutput.
        ProcessStartInfo processInfo = new ProcessStartInfo("git", @gitCommand)
        {
            CreateNoWindow = true,          // We want no visible pop-ups
            UseShellExecute = false,        // Allows us to redirect input, output and error streams
            RedirectStandardOutput = true,  // Allows us to read the output stream
            RedirectStandardError = true    // Allows us to read the error stream
        };

        // Set up the Process
        Process process = new Process
        {
            StartInfo = processInfo
        };

        try
        {
            process.Start();  // Try to start it, catching any exceptions if it fails
        }
        catch (Exception e)
        {
            // For now just assume its failed cause it can't find git.
            Debug.LogError("Git is not set-up correctly, required to be on PATH, and to be a git project.");
            throw e;
        }

        // Read the results back from the process so we can get the output and check for errors
        output = process.StandardOutput.ReadToEnd();
        errorOutput = process.StandardError.ReadToEnd();

        process.WaitForExit();  // Make sure we wait till the process has fully finished.
        process.Close();        // Close the process ensuring it frees it resources.

        // Check for failure due to no git setup in the project itself or other fatal errors from git.
        if (output.Contains("fatal") || output == "no-git" || output == "")
        {
            throw new Exception("Command: git " + @gitCommand + " Failed\n" + output + errorOutput);
        }
        // Log any errors.
        if (errorOutput != "")
        {
            Debug.LogError("Git Error: " + errorOutput);
        }

        return output;  // Return the output from git.
    }
}


// On branch master
// Your branch is ahead of 'Github/master' by 1 commit.
//   (use "git push" to publish your local commits)

// Changes to be committed:
//   (use "git restore --staged <file>..." to unstage)
// 	modified:   Assembly-CSharp-Editor.csproj

// Changes not staged for commit:
//   (use "git add <file>..." to update what will be committed)
//   (use "git restore <file>..." to discard changes in working directory)
//   (commit or discard the untracked or modified content in submodules)
// 	modified:   Assets/BicLIb (untracked content)


// UnityEngine.Debug:Log (object)
// GitUtil:LogCommit () (at Assets/BicLIb/GitVersions/Editor/GitUtil.cs:15)