# load the custom baseline script
@load scripts/test

# Load the known bad hashes dataset
@load frameworks/intel/seen
@load frameworks/intel/do_notice

# Load the community packages installed via zkg
@load packages

redef Intel::read_files += {
    "/usr/local/zeek/share/zeek/site/intel.dat"
};