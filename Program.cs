using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using DataUpdater.HCR.Core;
using DataUpdater.HCR.Types;
using System.Text.Unicode;
using System.Text;
using System.Drawing;
using System.Buffers.Text;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text.Json.Nodes;

string t =
    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjo1LCJleHAiOjE3NzcyNTg1MjksImlhdCI6MTc3NjY1MzcyOX0.iCUA6uf3eXmWIr_n-FtcbEjtDzBBgoLjW34_FMfMiC8";

var r = HcrService.GetTokenInfo(t);
Console.WriteLine(r.Uid);