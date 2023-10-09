using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public static class AuthenticationWrapper 
{
    //anyone can access, but it can only change on this class
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxTries = 5){
        if(AuthState == AuthState.Authenticated){
            return AuthState;
        }

        AuthState = AuthState.Authenticating;
        int tries = 0;
        while (AuthState == AuthState.Authenticating && tries < maxTries) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if(AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized) {
                AuthState = AuthState.Authenticated;
                break;
            }

            //tries again, but if it fails, wait one second
            tries++;
            await Task.Delay(1000);

        }

        return AuthState;
    }
}

public enum AuthState {
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    Timeout
}
