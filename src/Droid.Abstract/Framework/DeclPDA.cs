using System;
using System.Collections.Generic;

namespace Droid.Framework
{
    public class DeclEmail : Decl
    {
        //public DeclEmail() {}

        public override int Size() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();
        public override void Print() => throw new NotImplementedException();
        public override void List() => throw new NotImplementedException();

        public string GetFrom() => from;
        public string GetBody() => text;
        public string GetSubject() => subject;
        public string GetDate() => date;
        public string GetTo() => to;
        public string GetImage() => image;

        string text;
        string subject;
        string date;
        string to;
        string from;
        string image;
    }

    public class DeclVideo : Decl
    {
        //public DeclVideo() {};

        public override int Size() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();
        public override void Print() => throw new NotImplementedException();
        public override void List() => throw new NotImplementedException();

        public string GetRoq() => video;
        public string GetWave() => audio;
        public string GetVideoName() => videoName;
        public string GetInfo() => info;
        public string GetPreview() => preview;

        string preview;
        string video;
        string videoName;
        string info;
        string audio;
    }

    public class DeclAudio : Decl
    {
        //public DeclAudio() { };

        public override int Size() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();
        public override void Print() => throw new NotImplementedException();
        public override void List() => throw new NotImplementedException();

        public string GetAudioName() => audioName;
        public string GetWave() => audio;
        public string GetInfo() => info;
        public string GetPreview() => preview;

        string audio;
        string audioName;
        string info;
        string preview;
    }

    public class DeclPDA : Decl
    {
        //public DeclPDA() { originalEmails = originalVideos = null; }

        public override int Size() => throw new NotImplementedException();
        public override string DefaultDefinition() => throw new NotImplementedException();
        public override bool Parse(string text, int textLength) => throw new NotImplementedException();
        public override void FreeData() => throw new NotImplementedException();
        public override void Print() => throw new NotImplementedException();
        public override void List() => throw new NotImplementedException();

        public virtual void AddVideo(string name, bool unique = true) => throw new NotImplementedException();
        public virtual void AddAudio(string name, bool unique = true) => throw new NotImplementedException();
        public virtual void AddEmail(string name, bool unique = true) => throw new NotImplementedException();
        public virtual void RemoveAddedEmailsAndVideos() => throw new NotImplementedException();

        public virtual int GetNumVideos() => throw new NotImplementedException();
        public virtual int GetNumAudios() => throw new NotImplementedException();
        public virtual int GetNumEmails() => throw new NotImplementedException();
        public virtual DeclVideo GetVideoByIndex(int index) => throw new NotImplementedException();
        public virtual DeclAudio GetAudioByIndex(int index) => throw new NotImplementedException();
        public virtual DeclEmail GetEmailByIndex(int index) => throw new NotImplementedException();

        public virtual void SetSecurity(string sec) => throw new NotImplementedException();

        public string GetPdaName() => pdaName;
        public string GetSecurity() => security;
        public string GetFullName() => fullName;
        public string GetIcon() => icon;
        public string GetPost() => post;
        public string GetID() => id;
        public string GetTitle() => title;

        List<string> videos = new();
        List<string> audios = new();
        List<string> emails = new();
        string pdaName;
        string fullName;
        string icon;
        string id;
        string post;
        string title;
        string security;
        int originalEmails;
        int originalVideos;
    }
}