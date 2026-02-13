##! Modern Local Site Policy for Zeek
##! ---------------------------------

# 1. DEFINE YOUR NETWORK (CRITICAL)
# Replace these subnets with the actual IP range of your Docker network or Host LAN.
# Without this, Zeek cannot determine "Inbound" vs "Outbound".
redef Site::local_nets += { 
    172.16.0.0/12,    # Example: Docker Networks
    10.200.1.0/24        # Example: Private LAN
};

# 2. LOAD THE NOTICE FRAMEWORK
@load policy/frameworks/notice

# 4. ENABLE TRACEROUTE DETECTION
@load policy/misc/detect-traceroute

# 5. SECURITY & ANOMALIES (Modern Paths)
# Detect cleartext passwords/auth
@load policy/protocols/ftp/detect

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

##! ----------------------------------------------------------------------
##! CUSTOM SCAN DETECTION
##! ----------------------------------------------------------------------

# 1. Define the Notice Type
redef enum Notice::Type += {
    Port_Scan,
    Address_Scan
};

# 2. Configure Thresholds (Low for testing)
const scan_threshold_count = 5;       # How many distinct ports/hosts?
const scan_interval = 5mins;          # In what time window?

# 3. Define the Tracker (Who is scanning whom?)
global scanner_tracking: table[addr] of set[port] &create_expire=scan_interval;

# 4. The Logic
event connection_attempt(c: connection)
    {
    local src = c$id$orig_h;
    local dst_port = c$id$resp_p;

    # Initialize if new
    if ( src !in scanner_tracking )
        scanner_tracking[src] = set();

    # Add the port to the set
    add scanner_tracking[src][dst_port];

    # Check Threshold
    if ( |scanner_tracking[src]| == scan_threshold_count )
        {
        NOTICE([$note=Port_Scan,
                $msg=fmt("Host %s has scanned %d unique ports", src, |scanner_tracking[src]|),
                $src=src,
                $identifier=cat(src)]);
        }
    }