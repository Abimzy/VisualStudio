﻿using System;
using GitHub.App.Services;
using NUnit.Framework;

public class GitHubContextServiceTests
{
    public class TheFindContextFromUrlMethod
    {
        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", "github")]
        [TestCase("https://github.com/github/VisualStudio", "github")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "github")]
        public void Owner(string url, string expectOwner)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
        }

        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", null)]
        [TestCase("https://github.com/github/VisualStudio", "VisualStudio")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "VisualStudio")]
        public void RepositoryName(string url, string expectRepositoryName)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
        }

        [TestCase("https://github.com", "github.com")]
        [TestCase("https://github.com/github", "github.com")]
        [TestCase("https://github.com/github/VisualStudio", "github.com")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "github.com")]
        public void Host(string url, string expectHost)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Host, Is.EqualTo(expectHost));
        }

        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", null)]
        [TestCase("https://github.com/github/VisualStudio", null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", null)]
        [TestCase("https://github.com/github/VisualStudio/pull/1763", 1763)]
        [TestCase("https://github.com/github/VisualStudio/pull/1763/commits", 1763)]
        [TestCase("https://github.com/github/VisualStudio/pull/1763/files#diff-7384294e6c288e13bad0293bae232754R1", 1763)]
        [TestCase("https://github.com/github/VisualStudio/pull/NaN", null)]
        public void PullRequest(string url, int? expectPullRequest)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context?.PullRequest, Is.EqualTo(expectPullRequest));
        }

        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", null)]
        [TestCase("https://github.com/github/VisualStudio", null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "README.md")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md#notices", "README.md")]
        [TestCase("https://github.com/github/VisualStudio/blob/0d264d50c57d701fa62d202f481075a6c6dbdce8/src/Code.cs#L86", "src/Code.cs")]
        public void Path(string url, string expectPath)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Path, Is.EqualTo(expectPath));
        }

        // HACK: We're assuming that branches don't contain a '/' (sic)
        [TestCase("https://github.com/github/VisualStudio/blob/fixes/branch/buggy.cs", "branch/buggy.cs")]
        public void ProblemPath(string url, string expectPath)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Path, Is.EqualTo(expectPath));
        }

        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", null)]
        [TestCase("https://github.com/github/VisualStudio", null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md#notices", null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/src/GitHub.VisualStudio/GitHubPackage.cs#L38", 38)]
        [TestCase("https://github.com/github/VisualStudio/blob/0d264d50c57d701fa62d202f481075a6c6dbdce8/src/Code.cs#L86", 86)]
        public void Line(string url, int? expectLine)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Line, Is.EqualTo(expectLine));
        }

        [TestCase("foo", true)]
        [TestCase("ssh://git@github.com:443/benstraub/libgit2", true)]
        [TestCase("https://github.com/github/VisualStudio", false)]
        public void IsNull(string url, bool expectNull)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context, expectNull ? Is.Null : Is.Not.Null);
        }
    }

    public class TheToMethod
    {
        [Test]
        public void DefaultGitHubDotCom()
        {
            var context = new GitHubContext { Host = "github.com", Owner = "github", RepositoryName = "VisualStudio" };
            var target = new GitHubContextService();

            var uri = target.ToRepositoryUrl(context);

            Assert.That(uri, Is.EqualTo(new Uri("https://github.com/github/VisualStudio")));
        }
    }

    public class TheFindContextFromWindowTitleMethod
    {
        [TestCase("github/0123456789: Description - Google Chrome", "0123456789")]
        [TestCase("github/abcdefghijklmnopqrstuvwxyz: Description - Google Chrome", "abcdefghijklmnopqrstuvwxyz")]
        [TestCase("github/ABCDEFGHIJKLMNOPQRSTUVWXYZ: Description - Google Chrome", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("github/_: Description - Google Chrome", "_")]
        [TestCase("github/.: Description - Google Chrome", ".")]
        [TestCase("github/-: Description - Google Chrome", "-")]
        [TestCase("github/$: Description - Google Chrome", null, Description = "Must contain only letters, numbers, `_`, `.` or `-`")]
        public void RepositoryName(string windowTitle, string expectRepositoryName)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.RepositoryName, Is.EqualTo(expectRepositoryName));
        }

        [TestCase("0123456789/Repository: Description - Google Chrome", "0123456789")]
        [TestCase("abcdefghijklmnopqrstuvwxyz/Repository: Description - Google Chrome", "abcdefghijklmnopqrstuvwxyz")]
        [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ/Repository: Description - Google Chrome", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("a_/Repository: Description - Google Chrome", "a_")]
        [TestCase("a-/Repository: Description - Google Chrome", "a-")]
        [TestCase("_/Repository: Description - Google Chrome", null, Description = "Must start with letter or number")]
        [TestCase("-/Repository: Description - Google Chrome", null, Description = "Must start with letter or number")]
        public void Owner(string windowTitle, string expectOwner)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.Owner, Is.EqualTo(expectOwner));
        }

        // They can include slash / for hierarchical (directory) grouping
        [TestCase("a/b", "a/b", Description = "")]
        [TestCase("aaa/bbb", "aaa/bbb", Description = "")]

        // They cannot have space, tilde ~, caret ^, or colon : anywhere.
        [TestCase("a b", null)]
        [TestCase("a~b", null)]
        [TestCase("a^b", null)]
        [TestCase("a:b", null)]

        // They cannot have question-mark ?, asterisk *, or open bracket [ anywhere.
        [TestCase("a?b", null)]
        [TestCase("a*b", null)]
        [TestCase("a[b", null)]

        [TestCase(@"a\b", null, Description = @"They cannot contain a \")]

        // Simple case
        [TestCase("master", "master")]

        // There are many symbols they can contain
        [TestCase("!@#$%&()_+-=", "!@#$%&()_+-=")]

        [TestCase("/a", null, Description = "They cannot begin a slash")]
        [TestCase("a/", null, Description = "They cannot end with a slash")]
        [TestCase("../b", null, Description = "no slash-separated component can begin with a dot")]
        [TestCase(".a/b", null, Description = "no slash-separated component can begin with a dot")]
        [TestCase("a/.b", null, Description = "no slash-separated component can begin with a dot")]

        // There are some checks we aren't doing, see https://git-scm.com/docs/git-check-ref-format
        // They cannot have ASCII control characters(i.e.bytes whose values are lower than \040, or \177 DEL)        
        // [TestCase("a/b.lock", null, Description = "or end with the sequence.lock")]
        // [TestCase("a..b", null, Description = "They cannot have two consecutive dots..anywhere")]
        // [TestCase("a.", null, Description = "They cannot end with a dot")]
        // [TestCase("@{a", null, Description = "They cannot contain a sequence @{")]
        // [TestCase("@", null, Description = "They cannot be the single character @")]
        public void Branch(string branch, string expectBranch)
        {
            var windowTitle = $"VisualStudio/src/GitHub.VisualStudio/Resources/icons at {branch} · github/VisualStudio - Google Chrome";
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.Branch, Is.EqualTo(expectBranch));
        }

        [TestCase("github/VisualStudio: GitHub Extension for Visual Studio - Google Chrome", "github", "VisualStudio", null)]
        [TestCase("Branches · github/VisualStudio - Google Chrome", "github", "VisualStudio", null)]
        [TestCase("github/VisualStudio at build/appveyor-fixes - Google Chrome", "github", "VisualStudio", "build/appveyor-fixes")]
        [TestCase("[spike] Open from GitHub URL by jcansdale · Pull Request #1763 · github/VisualStudio - Google Chrome", "github", "VisualStudio", null)]
        [TestCase("Consider adding C# code style preferences to editorconfig · Issue #1750 · github/VisualStudio - Google Chrome", "github", "VisualStudio", null)]
        [TestCase("VisualStudio/mark_github.xaml at master · github/VisualStudio - Google Chrome", "github", "VisualStudio", "master")]
        [TestCase("VisualStudio/src/GitHub.VisualStudio/Resources/icons at master · github/VisualStudio - Google Chrome", "github", "VisualStudio", "master")]
        [TestCase("VisualStudio/GitHub.Exports.csproj at 89484dc25a3a475d3253afdc3bd3ddd6c6999c3b · github/VisualStudio - Google Chrome", "github", "VisualStudio", "89484dc25a3a475d3253afdc3bd3ddd6c6999c3b")]
        public void OwnerRepositoryBranch(string windowTitle, string expectOwner, string expectRepositoryName, string expectBranch)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
            Assert.That(context.Branch, Is.EqualTo(expectBranch));
        }

        [TestCase("github/VisualStudio at build/appveyor-fixes - Google Chrome", "github", "VisualStudio", "build/appveyor-fixes", Description = "Chrome")]
        [TestCase("GitHub - github/VisualStudio at refactor/pr-list - Mozilla Firefox", "github", "VisualStudio", "refactor/pr-list", Description = "Firefox")]
        public void TreeBranch(string windowTitle, string expectOwner, string expectRepositoryName, string expectBranch)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
            Assert.That(context.Branch, Is.EqualTo(expectBranch));
        }

        [TestCase("Branches · github/VisualStudio - Google Chrome", "github", "VisualStudio", Description = "Chrome")]
        [TestCase("Branches · github/VisualStudio · GitHub - Mozilla Firefox", "github", "VisualStudio", Description = "Firefox")]
        public void Branches(string windowTitle, string expectOwner, string expectRepositoryName)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
        }

        [TestCase("Description · Pull Request #1763 · github/VisualStudio - Google Chrome", 1763)]
        [TestCase("Description · Pull Request #1763 · github/VisualStudio · GitHub - Mozilla Firefox", 1763, Description = "Firefox")]
        public void PullRequest(string windowTitle, int expectPullRequest)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.PullRequest, Is.EqualTo(expectPullRequest));
        }

        [TestCase("Consider adding C# code style preferences to editorconfig · Issue #1750 · github/VisualStudio - Google Chrome", 1750)]
        [TestCase("Scrape browser titles · Issue #4 · jcansdale/VisualStudio · GitHub - Mozilla Firefox", 4, Description = "Firefox")]

        public void Issue(string windowTitle, int expectIssue)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Issue, Is.EqualTo(expectIssue));
        }

        [TestCase("VisualStudio/mark_github.xaml at master · github/VisualStudio - Google Chrome", "mark_github.xaml")]
        [TestCase("VisualStudio/src/GitHub.VisualStudio/Resources/icons at master · github/VisualStudio - Google Chrome", "src/GitHub.VisualStudio/Resources/icons")]
        [TestCase("VisualStudio/README.md at master · jcansdale/VisualStudio · GitHub - Mozilla Firefox", "README.md", Description = "Firefox")]
        public void Path(string windowTitle, string expectPath)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.Path, Is.EqualTo(expectPath));
        }

        [TestCase("jcansdale/VisualStudio: GitHub Extension for Visual Studio - Google Chrome", "jcansdale", "VisualStudio", Description = "Chrome")]
        [TestCase("GitHub - jcansdale/VisualStudio: GitHub Extension for Visual Studio - Mozilla Firefox", "jcansdale", "VisualStudio", Description = "Firefox")]
        public void RepositoryHome(string windowTitle, string expectOwner, string expectRepositoryName)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
        }
    }
}
