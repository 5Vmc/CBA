#import <AdSupport/AdSupport.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <CoreTelephony/CTTelephonyNetworkInfo.h>
#import <CoreTelephony/CTCarrier.h>

const char* AutonomousStringCopy_Push(const char* string)
{
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

//C#直接调用的C函数
#if defined (__cplusplus)
extern "C" {
#endif
    void RequireIDFA() {
        if (@available(iOS 14, *)) {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                if (status == ATTrackingManagerAuthorizationStatusAuthorized) {
                    printf("IDFA: %s\n", [[[ASIdentifierManager sharedManager].advertisingIdentifier UUIDString] cStringUsingEncoding:[NSString defaultCStringEncoding]]);
                }
                else {
                    printf("No IDFA\n");
                }
            }];
        } else {
            // Fallback on earlier versions
        }
    }

    int GetIDFAAuthorizationStatus() {
        if (@available(iOS 14, *)) {
            return (int)(ATTrackingManager.trackingAuthorizationStatus);
        } else {
            return 0;
        }
    }

    const char* GetIDFA() {
        return AutonomousStringCopy_Push([[[ASIdentifierManager sharedManager].advertisingIdentifier UUIDString] cStringUsingEncoding:[NSString defaultCStringEncoding]]);
    }

    void JumpToIDFASetting() {
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString]];
    }
    
    const char* GetSystemLanguage(){
        return AutonomousStringCopy_Push([[NSLocale preferredLanguages][0]
                                          cStringUsingEncoding:[NSString defaultCStringEncoding]]);
    }
    
    const char* GetSimCountryCode() {
        CTTelephonyNetworkInfo *info = [[CTTelephonyNetworkInfo alloc] init];
        if (info == NULL) {
            return AutonomousStringCopy_Push("");
        }
        
        CTCarrier *carrier = [info subscriberCellularProvider];
        if (carrier == NULL) {
            return AutonomousStringCopy_Push("");
        }
        
        NSString *code = carrier.isoCountryCode;
        if (code == NULL) {
            return AutonomousStringCopy_Push("");
        }
        
        return AutonomousStringCopy_Push([[code uppercaseString] cStringUsingEncoding:(NSUTF8StringEncoding)]);
    }
    
    const char* GetCountryCode(){
        NSLocale *locale = [NSLocale currentLocale]; 
        NSString *countryCode = [locale objectForKey:NSLocaleCountryCode];

        const char *countrycodeStr = NULL;
        if ([countryCode canBeConvertedToEncoding:NSUTF8StringEncoding]) {
        
            countrycodeStr = [countryCode cStringUsingEncoding:NSUTF8StringEncoding];
        }
        return AutonomousStringCopy_Push(countrycodeStr);
    }
    
    char* GetCountryName(){
        return NULL;
    }
    
    void QuitApp(){
        int *locale = NULL;
        *locale = "1"; 
    }
    

#if defined (__cplusplus)
}
#endif