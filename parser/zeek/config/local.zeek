# load the custom baseline script
@load scripts/test.zeek

# Load the community packages installed via zkg
@load packages

# Load the known bad hashes dataset
@load frameworks/intel/seen
@load frameworks/intel/do_notice
redef Intel::read_files += {
    "/usr/local/zeek/share/zeek/site/intel.dat"
};

# Load Zeek's native SSH brute-force detection heuristics
@load protocols/ssh/detect-bruteforcing
redef SSH::password_guesses_limit = 5;
redef SSH::guessing_timeout = 10 mins;