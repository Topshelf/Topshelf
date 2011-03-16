COPYRIGHT = "Copyright 2007-2011 Travis Smith, Chris Patterson, Dru Sellers, Henrik Feldt et al. All rights reserved."

require File.dirname(__FILE__) + "/build_support/BuildUtils.rb"

include FileTest
require 'albacore'
require File.dirname(__FILE__) + "/build_support/ilmergeconfig.rb"
require File.dirname(__FILE__) + "/build_support/ILMerge.rb"

BUILD_NUMBER_BASE = '2.2.1'
PRODUCT = 'Topshelf'
CLR_TOOLS_VERSION = 'v4.0.30319'

BUILD_CONFIG = ENV['BUILD_CONFIG'] || "Release"
BUILD_CONFIG_KEY = ENV['BUILD_CONFIG_KEY'] || 'NET40'
BUILD_PLATFORM = ENV['BUILD_PLATFORM'] || 'x86' # we might want to vary this a little
TARGET_FRAMEWORK_VERSION = (BUILD_CONFIG_KEY == "NET40" ? "v4.0" : "v3.5")
MSB_USE = (BUILD_CONFIG_KEY == "NET40" ? :net4 : :net35)
OUTPUT_PATH = (BUILD_CONFIG_KEY == "NET40" ? 'net-4.0' : 'net-3.5')

props = { 
	:build_support => File.expand_path("build_support"),
    :stage => File.expand_path("build_output"),
    :output => File.join( File.expand_path("build_output"), OUTPUT_PATH ),
    :artifacts => File.expand_path("build_artifacts"),
	:projects => ["Topshelf", "Topshelf.Host"]
}

puts "Building for .NET Framework #{TARGET_FRAMEWORK_VERSION}."
 
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
task :default => [:clean, :compile, :ilmerge, :tests, :prepare_examples]

desc "**DO NOT CLEAR OUTPUT FOLDER**, compiles and runs tests"
task :unclean => [:compile, :ilmerge, :tests, :prepare_examples]

desc "Update the common version information for the build. You can call this task without building."
assemblyinfo :global_version do |asm|
  asm_version = BUILD_NUMBER_BASE + ".0"
  
  begin
    commit = `git log -1 --pretty=format:%H`
	commit_date = `git log -1 --pretty=format:%ai`
  rescue
    commit = "git unavailable"
  end
  build_number = "#{BUILD_NUMBER_BASE}.#{Date.today.strftime('%y%j')}"
  tc_build_number = ENV["BUILD_NUMBER"]
  puts "##teamcity[buildNumber '#{build_number}-#{tc_build_number}']" unless tc_build_number.nil?
  
  # Assembly file config
  asm.product_name = PRODUCT
  asm.description = "Git commit hash: #{commit} - #{commit_date} - Topshelf is an open source project for hosting services without friction. Either link Topshelf to your program and it *becomes* a service installer or use Topshelf.Host to shelf your services by placing them in subfolders of the 'Services' folder under the folder of Topshelf.Host.exe. github.com/Topshelf. topshelf-project.com. Original author company: CFT & ACM."
  asm.version = asm_version
  asm.file_version = build_number
  asm.custom_attributes :AssemblyInformationalVersion => "#{asm_version}",
	:ComVisibleAttribute => false,
	:CLSCompliantAttribute => false # Henrik: at the moment the project isn't CLS compliant due to dependencies.
  asm.copyright = COPYRIGHT
  asm.output_file = 'src/SolutionVersion.cs'
  asm.namespaces "System", "System.Reflection", "System.Runtime.InteropServices", "System.Security"
end

desc "Prepares the working directory for a new build"
task :clean do
	#TODO: do any other tasks required to clean/prepare the working directory
	FileUtils.rm_rf props[:stage]
	# work around latency issue where folder still exists for a short while after it is removed
	waitfor { !exists?(props[:stage]) }
	Dir.mkdir props[:stage]
	Dir.mkdir props[:artifacts] unless exists?(props[:artifacts])
end

desc "Cleans, versions, compiles the application and generates build_output/."
task :compile => [:global_version, :run_msbuild, :run_msbuildx86] do
	puts 'Copying unmerged dependencies to output folder'
	copyOutputFiles "src/Topshelf.Host/bin/#{BUILD_CONFIG}", "log4net.{dll,pdb,xml,config}", props[:output]
	copyOutputFiles "src/Topshelf.Host/bin/#{BUILD_CONFIG}", "Topshelf.Host.{exe,pdb}", props[:output]
	copyOutputFiles "src/Topshelf.Host/bin/#{BUILD_CONFIG}", "Topshelf.Host.exe.config", props[:output]
	copy("src/Topshelf.Host/bin/#{BUILD_CONFIG}-x86/Topshelf.Host.exe", File.join(props[:output], 'Topshelf.Host-x86.exe'))
	copy("src/Topshelf.Host/bin/#{BUILD_CONFIG}-x86/Topshelf.Host.pdb", File.join(props[:output], 'Topshelf.Host-x86.pdb'))
end

ilmerge :ilmerge do |ilm|
	out = File.join(props[:output], 'Topshelf.dll')
	ilm.output = out
	ilm.internalize = File.join(props[:build_support], 'internalize.txt')
	ilm.working_directory = "src/Topshelf/bin/#{BUILD_CONFIG}"
	ilm.target = :library
    ilm.use MSB_USE
	ilm.log = File.join( File.expand_path("src"), "Topshelf","bin","#{BUILD_CONFIG}", 'ilmerge.log' )
	ilm.allow_dupes = true
	ilm.references = [ 'Topshelf.dll', 'Magnum.dll', 'Newtonsoft.Json.dll', 'Spark.dll', 'Stact.dll', 'Stact.ServerFramework.dll' ]
end

desc "Prepare examples"
task :prepare_examples => [:compile] do
	puts "Preparing samples"
	targ = File.join(props[:output], 'Services', 'clock' )
	copyOutputFiles "src/Samples/StuffOnAShelf/bin/#{BUILD_CONFIG}", "clock.*", targ
	copyOutputFiles "src/Samples/StuffOnAShelf/bin/#{BUILD_CONFIG}", "StuffOnAShelf.{dll}", targ
	copyOutputFiles props[:output], "Topshelf.{dll}", targ
	copyOutputFiles props[:output], "log4net.{dll,pdb}", targ
	copy('doc/Using Shelving.txt', props[:output])
end

desc "Only compiles the application."
msbuild :run_msbuild do |msb|
	msb.properties :Configuration => BUILD_CONFIG, 
		:TargetFrameworkVersion => TARGET_FRAMEWORK_VERSION,
		:Platform => 'Any CPU'
	msb.use MSB_USE
	msb.targets :Clean, :Build
	msb.solution = 'src/Topshelf.sln'
end

desc "Only compiles the application."
msbuild :run_msbuildx86 do |msb|
	msb.properties :Configuration => BUILD_CONFIG, 
		:TargetFrameworkVersion => TARGET_FRAMEWORK_VERSION,
		:Platform => 'x86'
	msb.use MSB_USE
	msb.targets :Clean, :Build
	msb.solution = 'src/Topshelf.sln'
end

def copyOutputFiles(fromDir, filePattern, outDir)
	FileUtils.mkdir_p outDir unless exists?(outDir)
	Dir.glob(File.join(fromDir, filePattern)){|file|
		copy(file, outDir) if File.file?(file)
	}
end

task :tests => [:unit_tests, :integration_tests, :perf_tests]

desc "Runs unit tests (integration tests?, acceptance-tests?) etc."
task :unit_tests => [:compile] do
	Dir.mkdir props[:artifacts] unless exists?(props[:artifacts])

	runner = NUnitRunner.new(File.join('lib', 'nunit', 'net-2.0',  "nunit-console#{(BUILD_PLATFORM.empty? ? '' : "-#{BUILD_PLATFORM}")}.exe"),
		'tests',
		TARGET_FRAMEWORK_VERSION,
		['/nothread', '/nologo', '/labels', "\"/xml=#{File.join(props[:artifacts], 'nunit-test-results.xml')}\""])

	runner.run ['Topshelf.Specs'].map{ |assem| "#{assem}.dll" }
end

desc "Runs the integation tests"
task :integration_tests => [:prepare_examples] do 
	puts "TODO: Integration tests."
end

desc "Runs the performance tests (a form of integation tests arguably)."
task :perf_tests => [:compile] do
	puts "TODO: Performance tests."
end

desc "Target used for the CI server. It both builds, tests and packages."
task :ci => [:default, :package, :moma]

desc "ZIPs up the build results and runs the MoMA analyzer."
zip :package do |zip|
	zip.directories_to_zip = [props[:stage]]
	zip.output_file = 'topshelf.zip'
	zip.output_path = [props[:artifacts]]
end

desc "Runs the MoMA mono analyzer on the project files. Start the executable manually without --nogui to update the profiles once in a while though, or you'll always get the same report from the analyzer."
task :moma => [:compile] do
	puts "Analyzing project fitness for mono:"
	dlls = project_outputs(props).join(' ')
	sh "lib/MoMA/MoMA.exe --nogui --out #{File.join(props[:artifacts], 'MoMA-report.html')} #{dlls}"
end

desc "Builds the nuget package"
task :nuget do
#	sh "lib/nuget.exe pack packaging/nuget/topshelf.nuspec -o artifacts"
end

def project_outputs(props)
	props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.dll" }.
		concat( props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.exe" } ).
		find_all{ |path| exists?(path) }
end

def waitfor(&block)
	checks = 0
	
	until block.call || checks >10 
		sleep 0.5
		checks += 1
	end
	
	raise 'waitfor timeout expired' if checks > 10
end
