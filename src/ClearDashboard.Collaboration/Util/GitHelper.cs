using System;
using LibGit2Sharp;
using System.IO;
using Microsoft.Extensions.Logging;
using ClearDashboard.DataAccessLayer.Models;
using System.Text.Json;
using ClearDashboard.Collaboration.Model;

namespace ClearDashboard.Collaboration.Util;

public class GitHelper
{
    public static bool IsValid(string path, ILogger logger)
    {
        return Repository.IsValid(path);
    }

    public static void EnsureInit(string path, ILogger logger)
    {
        if (!Repository.IsValid(path))
        {
            Repository.Init(path);
        }
    }

    public static void GitStage(string repositoryPath, string projectPath, ILogger logger)
    {
        using (var repo = new Repository(repositoryPath))
        {
            Commands.Stage(repo, projectPath);
        }
    }

    public static void LsTree(string repositoryPath, ILogger logger)
    {
        using (var repo = new Repository(repositoryPath))
        {
            var commitish = "07ebd80";
            var projectFolder = "Project_1e71b87a-fadf-424f-a3e0-e427a37d5d7e";
            var tree = repo.Lookup<Tree>($"{commitish}:{projectFolder}");
            foreach (var item in tree)
                logger.LogInformation($"{item.TargetType} - {item.Target}:  {item.Name} - {item.Path}");
        }
    }

    public static IEnumerable<TreeEntry> LsTreeProjectSubfolder(string repositoryPath, string commitSha1, Guid projectId, string subfolder, ILogger logger)
    {
        using (var repo = new Repository(repositoryPath))
        {
            var tree = repo.Lookup<Tree>($"{commitSha1}:Project_{projectId}/{subfolder}");
            return tree
                .OrderBy(t => t.Name)
                .ToList();
        }
    }

    public static string CatFile(string repositoryPath, string commitish, Guid projectId, string subfolder, ILogger logger)
    {
        using (var repo = new Repository(repositoryPath))
        {
            var blob = repo.Lookup<Blob>($"{commitish}:Project_{projectId}/{subfolder}");
            return blob.GetContentText();
        }
    }

    public static void GitCommit(string repositoryPath, ILogger logger)
    {
        using (var repo = new Repository(repositoryPath))
        {
            repo.Commit(
                "zboo",
                new Signature("chris", "chris.morley@clear.bible", DateTimeOffset.UtcNow),
                new Signature("chris", "chris.morley@clear.bible", DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    /// 07ebd80d9e1615bc6229c17a6a19740ab126587e (first commit)
    /// 061f04bdaf390db4d5c61c1ff5461ef0e7dcc70f (second commit)
    /// </summary>
    /// <param name="repositoryPath"></param>
    /// <param name="logger"></param>
    public static void AddRemoteOrigin(string repositoryPath, ILogger logger)
    {
        using (var repo = new Repository(repositoryPath))
        {
            const string name = "origin";
            const string url = "https://github.com/morleycb/snapshot_test.git";

            repo.Network.Remotes.Add(name, url);
            Remote remote = repo.Network.Remotes[name];

            var refSpec = repo.Config.Get<string>("remote", remote.Name, "fetch");
        }
    }

    public static void GitPushToOrigin(string repositoryPath, ILogger logger)
    {
        using (var repo = new Repository(repositoryPath))
        {
            Remote remote = repo.Network.Remotes["origin"];
            var options = new PushOptions();
            options.CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials { Username = "morleycb", Password = "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" };
            repo.Network.Push(remote, @"refs/heads/master", options);
        }
    }

    public static void RetrieveStatus(string path, ILogger logger, params FileStatus[] fileStatuses)
    {
        using (var repo = new Repository(path))
        {
            foreach (var item in repo.RetrieveStatus(new LibGit2Sharp.StatusOptions()))
            {
                if (fileStatuses.Where(fs => item.State.HasFlag(fs)).Any())
                {
                    logger.LogInformation($"{item.State}:  {item.FilePath}");
                }
            }
        }
    }
}

