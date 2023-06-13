using System;
using System.Collections.Generic;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.Constants;

namespace PassMeta.DesktopApp.Ui.Views.Previews.Data;

/// <summary>
/// Preview data.
/// </summary>
internal static class PassFilePreviewData
{
    public static PwdSection PwdSection => new()
    {
        Id = Guid.NewGuid(),
        Name = "My section",
        WebsiteUrl = "url.net/my",
        Items = new List<PwdItem>
        {
            new()
            {
                Usernames = new[] { "my-username" },
                Password = "passss",
                Remark = "remaaaark",
            },
            new()
            {
                Usernames = new[]
                {
                    "my-username1",
                    "my-username2",
                },
                Password = "passssssssssss",
                Remark = "remaaaarkkkk",
            },
        }
    };

    public static TxtSection TxtSection => new()
    {
        Id = Guid.NewGuid(),
        Name = "My section",
        Content = "Some content",
    };

    public static PassFile GetPassFile() => GetPassFile<PwdPassFile>();

    public static TPassFile GetPassFile<TPassFile>()
        where TPassFile : PassFile
    {
        if (typeof(TPassFile) == typeof(PwdPassFile))
        {
            return (new PwdPassFile
            {
                Id = 123,
                Color = PassFileColor.Green.Hex,
                Name = "My passfile",
                CreatedOn = DateTime.Now,
                InfoChangedOn = DateTime.Now,
                VersionChangedOn = DateTime.Now,
                Version = 1,
                OriginChangeStamps = new PwdPassFile
                {
                    CreatedOn = DateTime.Now.AddDays(-1),
                    InfoChangedOn = DateTime.Now.AddDays(-2),
                    VersionChangedOn = DateTime.Now.AddDays(-3),
                },
                Content = new PassFileContent<List<PwdSection>>(
                    new List<PwdSection> { PwdSection, PwdSection }, "password")
            } as TPassFile)!;
        }

        if (typeof(TPassFile) == typeof(TxtPassFile))
        {
            return (new TxtPassFile
            {
                Id = 123,
                Color = PassFileColor.Green.Hex,
                Name = "My passfile",
                CreatedOn = DateTime.Now,
                InfoChangedOn = DateTime.Now,
                VersionChangedOn = DateTime.Now,
                Version = 1,
                OriginChangeStamps = new PwdPassFile
                {
                    CreatedOn = DateTime.Now.AddDays(-1),
                    InfoChangedOn = DateTime.Now.AddDays(-2),
                    VersionChangedOn = DateTime.Now.AddDays(-3),
                },
                Content = new PassFileContent<List<TxtSection>>(
                    new List<TxtSection> { TxtSection, TxtSection }, "password")
            } as TPassFile)!;
        }

        throw new ArgumentOutOfRangeException(nameof(TPassFile), typeof(TPassFile), null);
    }
}