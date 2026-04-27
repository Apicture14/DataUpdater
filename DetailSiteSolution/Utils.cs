using System;

public class LoginRequiredAttribute : Attribute
{
    public string message => "需要登录";
}