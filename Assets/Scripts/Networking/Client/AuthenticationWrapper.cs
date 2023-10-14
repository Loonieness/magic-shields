using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper 
{
    //anyone can access, but it can only change on this class
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxRetries = 5){
        if(AuthState == AuthState.Authenticated){
            return AuthState;
        }

        if(AuthState == AuthState.Authenticating){
            Debug.LogWarning("Already Authenticating");
            await Authenticating();
            return AuthState;
        }

        await SignInAnonymouslyAsync(maxRetries);
        
        return AuthState;
    }

    private static async Task<AuthState> Authenticating(){
        while(AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated){
            await Task.Delay(200);
        }

        return AuthState;
    }


    private static async Task SignInAnonymouslyAsync(int maxRetries){
        AuthState = AuthState.Authenticating;
        int retries = 0;
        while (AuthState == AuthState.Authenticating && retries < maxRetries) {
            try
            {
                //tries to use the Unity sign in method, not this actual class again
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if(AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized) {
                AuthState = AuthState.Authenticated;
                break;
            }
                
            }
            catch (AuthenticationException authException)
            {
                Debug.LogError(authException);
                AuthState = AuthState.Error;
            }
            //can happen if there's no connection or can't connect to server
            catch (RequestFailedException RequestException){
                Debug.LogError(RequestException);
                AuthState = AuthState.Error;
            }

            //tries again, but if it fails, wait one second
            retries++;
            await Task.Delay(1000);

        }

        if(AuthState != AuthState.Authenticated){
            Debug.LogWarning($"Player was not signed in successfully after {retries} retries");
            AuthState = AuthState.Timeout;
        }


    }
}

public enum AuthState {
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    Timeout
}
