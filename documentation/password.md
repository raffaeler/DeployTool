# Encrypt and decrypt a password
Managing clear-text passwords is evil and you should never do it.

It happened many times in the past (and still happens) that open source repositories contain important secrets just because they were hard-coded or part of the configuration files.

The best way to manage the credentials in ssh is using the key files. Key files are supported by `deployssh` and can be specified in the configuration.

This document explains how to use the username/password authentication while not storing clear-text password in the configuration file.
Be warned that, even if SSH performs the password authentication over a secure channel (encrypted), it cannot certify that the target server is legit. With a DNS spoofing attack, it is possible for the attacker to claim being the server side and receive the password.

## Rationale
This tool use the Windows DPAPI to encrypt the clear-text password in a hexadecimal string and vice-versa.
The Windows DPAPI can decrypt the hexadecimal string into the password only if it has been encrypted on the same **machine** and the same **user profile**.

In other words, if you create the hexadecimal string on a machine and you decrypt it on another machine, the decryption will fail.

## Encryption
Generating an encrypted password is straightforward:
```
deployssh encrypt
Type the secret to encrypt. Echo is disabled for security reasons.
```
You now type the password. For security reasons the console will not echo the typed characters.
The output will be something like this:
```
Protect values are valid *only* for the current user profile
Write the encrypted value in the configuration
Use the decrypt command with this data to see the clear text value

Encrypted:
01000000D08C9DDF0115D1118C7A00C04FC297EB01000000CF3FED45FD607A41AB4EF5BFF8BF98F80000000002000000000003660000C00000001000000086491E1A684DC9A03EBF9E3A1C71BB200000000004800000A0000000100000002B90B3FEE8407D49D85DF293A209C19A100000007738864DC6DD1EC01821C117D7B2DB44140000004D04DB4261DAA7FA77B3EC3D2CA4AA3857172EE0
```

The hexadecimal string is intended for the configuration file and can be specified in the **Ssh/EncryptedPassword** value of the configuration file:
```
"EncryptedPassword": "01000000D08C9DDF0115D1118C7A00C04FC297EB01000000CF3FED45FD607A41AB4EF5BFF8BF98F80000000002000000000003660000C00000001000000086491E1A684DC9A03EBF9E3A1C71BB200000000004800000A0000000100000002B90B3FEE8407D49D85DF293A209C19A100000007738864DC6DD1EC01821C117D7B2DB44140000004D04DB4261DAA7FA77B3EC3D2CA4AA3857172EE0"
```

## Decryption
The tool also offers the chance to verify the encrypted password can be reversed correctly.

```
deployssh decrypt
No csproj found in H:\util
Type or paste the encrypted value. Be warned the clear text secret will be printed.
Decrypting is possible only if done using the user profile where they have been encrypted
```
The tool prints two important reminders and wait for some text to by typed or pasted from the Clipboard.

Now paste the value obtained from the encryption and press `Enter`.
The tool will print the clear-text password.
```
01000000D08C9DDF0115D1118C7A00C04FC297EB01000000CF3FED45FD607A41AB4EF5BFF8BF98F80000000002000000000003660000C00000001000000086491E1A684DC9A03EBF9E3A1C71BB200000000004800000A0000000100000002B90B3FEE8407D49D85DF293A209C19A100000007738864DC6DD1EC01821C117D7B2DB44140000004D04DB4261DAA7FA77B3EC3D2CA4AA3857172EE0
Decrypted:
password
```

