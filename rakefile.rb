COMPILE_TARGET = ENV['config'].nil? ? "debug" : ENV['config']
require File.dirname(__FILE__) + "/build_support/BuildUtils.rb"

include FileTest
require 'albacore'

RESULTS_DIR = "results"
BUILD_NUMBER_BASE = "2.2.0"
PRODUCT = "Topshelf"
COPYRIGHT = 'Copyright 2007-2011 Travis Smith, Chris Patterson, Dru Sellers, et al. All rights reserved.';
COMMON_ASSEMBLY_INFO = 'src/CommonAssemblyInfo.cs';
CLR_TOOLS_VERSION = "v4.0.30319"
# Either "NET35" or "NET40".
BUILD_CONFIG_KEY = "NET40"

props = { 
    :stage => File.expand_path("build_output"),
    :stage_merged => File.expand_path("build_merged"), 
    :artifacts => File.expand_path("build_artifacts"),
	:target_framework_version => (BUILD_CONFIG_KEY == "NET40" ? "v4.0" : "v3.5")
}

 

desc "Displays a list of tasks"
task :help do
  taskHash = Hash[*(`rake.bat -T`.split(/\n/).collect { |l| l.match(/rake (\S+)\s+\#\s(.+)/).to_a }.collect { |l| [l[1], l[2]] }).flatten] 
 
  indent = "                          "
  
  puts "rake #{indent}#Runs the 'default' task"
  
  taskHash.each_pair do |key, value|
    if key.nil?  
      next
    end
    puts "rake #{key}#{indent.slice(0, indent.length - key.length)}##{value}"
  end
end

desc "Compiles, unit tests, generates the database"
task :all => [:default]

desc "**Default**, compiles and runs tests"
task :default => [:compile, :unit_test]

desc "Update the version information for the build"
assemblyinfo :version do |asm|
  asm_version = BUILD_NUMBER_BASE + ".0"
  
  begin
    commit = `git log -1 --pretty=format:%H`
  rescue
    commit = "git unavailable"
  end
  build_number = "#{BUILD_NUMBER_BASE}.#{Date.today.strftime('%y%j')}"
  tc_build_number = ENV["BUILD_NUMBER"]
  puts "##teamcity[buildNumber '#{build_number}-#{tc_build_number}']" unless tc_build_number.nil?
  asm.trademark = commit
  asm.product_name = PRODUCT
  asm.description = build_number
  asm.version = asm_version
  asm.file_version = build_number
  asm.custom_attributes :AssemblyInformationalVersion => asm_version
  asm.copyright = COPYRIGHT
  asm.output_file = COMMON_ASSEMBLY_INFO
end

desc "Prepares the working directory for a new build"
task :clean do
	#TODO: do any other tasks required to clean/prepare the working directory
	FileUtils.rm_rf props[:stage]
    # work around nasty latency issue where folder still exists for a short while after it is removed
    waitfor { !exists?(props[:stage]) }
	Dir.mkdir props[:stage]
    
	Dir.mkdir props[:artifacts] unless exists?(props[:artifacts])
end

def waitfor(&block)
  checks = 0
  until block.call || checks >10 
    sleep 0.5
    checks += 1
  end
  raise 'waitfor timeout expired' if checks > 10
end

desc "Compiles the app"
task :compile => [:clean, :version] do
  MSBuildRunner.compile :compilemode => COMPILE_TARGET, 
	:solutionfile => 'src/Topshelf.sln', 
	:clrversion => CLR_TOOLS_VERSION,
	:properties => ["BuildConfigKey=#{BUILD_CONFIG_KEY}", 
					"TargetFrameworkVersion=#{props[:target_framework_version]}"]
  
  copyOutputFiles "bin/", "*.{dll,pdb}", props[:stage]
end

def copyOutputFiles(fromDir, filePattern, outDir)
  Dir.glob(File.join(fromDir, filePattern)){|file| 		
	copy(file, outDir) if File.file?(file)
  } 
end

desc "Runs unit tests"
task :test => [:unit_test]

desc "Runs unit tests"
task :unit_test => :compile do
  runner = NUnitRunner.new :compilemode => COMPILE_TARGET, 
	:source => 'src', 
	:platform => 'x86',
	:target_framework_version => props[:target_framework_version]
	
  runner.executeTests ['Topshelf.Specs']
end

desc "Target used for the CI server"
task :ci => [:default, :package]

desc "ZIPs up the build results"
zip :package do |zip|
	zip.directories_to_zip = [props[:stage]]
	zip.output_file = 'topshelf.zip'
	zip.output_path = [props[:artifacts]]
end

desc "Build the nuget package"
task :nuget do
#	sh "lib/nuget.exe pack packaging/nuget/topshelf.nuspec -o artifacts"
end