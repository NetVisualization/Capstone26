@load base/frameworks/notice

module Tripwire;

export {
    # Define a custom Notice type
    redef enum Notice::Type += {
        Suspicious_Port_Access
    };
}

# Event: Fires on every new TCP connection attempt
event connection_state_remove(c: connection)
{
    # Check if the destination port is 31337
    if ( c$id$resp_p == 31337/tcp )
    {
        # IMMEDIATELY generate a notice. No counting, no thresholds.
        NOTICE([$note=Suspicious_Port_Access,
                $msg=fmt("Tripwire triggered by %s scanning port 31337", c$id$orig_h),
                $conn=c,
                $identifier=cat(c$id$orig_h, c$id$resp_h, c$id$resp_p)]);
    }
}