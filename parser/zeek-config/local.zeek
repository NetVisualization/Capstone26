##! Modern Local Site Policy for Zeek
##! ---------------------------------

# 1. DEFINE YOUR NETWORK (CRITICAL)
# Replace these subnets with the actual IP range of your Docker network or Host LAN.
# Without this, Zeek cannot determine "Inbound" vs "Outbound".
redef Site::local_nets += { 
    192.168.1.0/24,   # Example: Your Home Network
    172.16.0.0/12,    # Example: Docker Networks
    10.0.0.0/8        # Example: Private LAN
};

# 2. LOAD THE NOTICE FRAMEWORK
@load policy/frameworks/notice

# 3. ENABLE SCAN DETECTION (The "Nmap" Detector)
@load policy/misc/scan

# Tune the thresholds so you see alerts immediately during testing
redef Scan::scan_threshold = 5;      # Alert after 5 attempts (Default is 25)
redef Scan::port_scan_threshold = 5; # Alert after 5 unique ports

# 4. ENABLE TRACEROUTE DETECTION
@load policy/misc/detect-traceroute

# 5. SECURITY & ANOMALIES (Modern Paths)
# Detect cleartext passwords/auth
@load policy/protocols/ftp/detect
@load policy/protocols/http/detect-sqli 

# Asset Tracking (Logs known hosts/services)
@load policy/protocols/conn/known-hosts
@load policy/protocols/conn/known-services
@load policy/protocols/ssl/validate-certs

# 6. SOFTWARE TRACKING
@load policy/frameworks/software/vulnerable
@load policy/frameworks/software/version-changes

# 7. EXTENDED LOGGING
# Log hash of all files transferred (great for forensics)
@load policy/frameworks/files/hash-all-files