COPYRIGHT = "Copyright 2012 Chris Patterson, Dru Sellers, Travis Smith, All rights reserved."

require File.dirname(__FILE__) + "/build_support/BuildUtils.rb"
require File.dirname(__FILE__) + "/build_support/util.rb"
include FileTest
require 'albacore'
require File.dirname(__FILE__) + "/build_support/versioning.rb"

PRODUCT = 'Topshelf'
CLR_TOOLS_VERSION = 'v4.0.30319'
OUTPUT_PATH = 'bin/Release'

props = {
  :src => File.expand_path("src"),
  :output => File.expand_path("build_output"),
  :artifacts => File.expand_path("build_artifacts"),
  :lib => File.expand_path("lib"),
  :projects => ["Topshelf"]
}

desc "Cleans, compiles, il-merges, unit tests, prepares examples, packages zip"
task :all => [:default, :package]

desc "**Default**, compiles and runs tests"
task :default => [:clean, :nuget_restore, :compile, :package]

desc "Update the common version information for the build. You can call this task without building."
assemblyinfo :global_version do |asm|
  # Assembly file config
  asm.product_name = PRODUCT
  asm.description = "Topshelf is an open source project for hosting services without friction. By referencing Topshelf, your console application *becomes* a service installer with a comprehensive set of command-line options for installing, configuring, and running your application as a service."
  asm.version = FORMAL_VERSION
  asm.file_version = FORMAL_VERSION
  asm.custom_attributes :AssemblyInformationalVersion => "#{BUILD_VERSION}",
	:ComVisibleAttribute => false,
	:CLSCompliantAttribute => true
  asm.copyright = COPYRIGHT
  asm.output_file = 'src/SolutionVersion.cs'
  asm.namespaces "System", "System.Reflection", "System.Runtime.InteropServices"
end

desc "Prepares the working directory for a new build"
task :clean do
	FileUtils.rm_rf props[:output]
	waitfor { !exists?(props[:output]) }

	FileUtils.rm_rf props[:artifacts]
	waitfor { !exists?(props[:artifacts]) }

	Dir.mkdir props[:output]
	Dir.mkdir props[:artifacts]
end

desc "Cleans, versions, compiles the application and generates build_output/."
task :compile => [:versioning, :global_version, :build4, :tests4, :copy4, :build35, :tests35, :copy35]

task :copy35 => [:build35] do
  copyOutputFiles File.join(props[:src], "Topshelf/bin/Release/v3.5"), "Topshelf.{dll,pdb,xml}", File.join(props[:output], 'net-3.5')
  copyOutputFiles File.join(props[:src], "Topshelf.Log4Net/bin/Release/v3.5"), "Topshelf.Log4Net.{dll,pdb,xml}", File.join(props[:output], 'net-3.5')
  copyOutputFiles File.join(props[:src], "Topshelf.NLog/bin/Release/v3.5"), "Topshelf.NLog.{dll,pdb,xml}", File.join(props[:output], 'net-3.5')
  copyOutputFiles File.join(props[:src], "Topshelf.Rehab/bin/Release/v3.5"), "Topshelf.Rehab.{dll,pdb,xml}", File.join(props[:output], 'net-3.5')
	copyOutputFiles File.join(props[:src], "Topshelf.Supervise/bin/Release/v3.5"), "Topshelf.Supervise.{dll,pdb,xml}", File.join(props[:output], 'net-3.5')
end

task :copy4 => [:build4] do
  copyOutputFiles File.join(props[:src], "Topshelf/bin/Release"), "Topshelf.{dll,pdb,xml}", File.join(props[:output], 'net-4.0-full')
  copyOutputFiles File.join(props[:src], "Topshelf.Log4Net/bin/Release"), "Topshelf.Log4Net.{dll,pdb,xml}", File.join(props[:output], 'net-4.0-full')
  copyOutputFiles File.join(props[:src], "Topshelf.NLog/bin/Release"), "Topshelf.NLog.{dll,pdb,xml}", File.join(props[:output], 'net-4.0-full')
  copyOutputFiles File.join(props[:src], "Topshelf.Rehab/bin/Release"), "Topshelf.Rehab.{dll,pdb,xml}", File.join(props[:output], 'net-4.0-full')
	copyOutputFiles File.join(props[:src], "Topshelf.Supervise/bin/Release"), "Topshelf.Supervise.{dll,pdb,xml}", File.join(props[:output], 'net-4.0-full')
end

desc "Only compiles the application."
msbuild :build35 do |msb|
	msb.properties :Configuration => "Release",
		:Platform => 'Any CPU',
                :TargetFrameworkVersion => "v3.5"
	msb.use :net35
	msb.targets :Clean, :Build
	msb.solution = 'src/Topshelf.sln'
end

desc "Only compiles the application."
msbuild :build4 do |msb|
	msb.properties :Configuration => "Release",
		:Platform => 'Any CPU'
	msb.use :net4
	msb.targets :Clean, :Build
	msb.solution = 'src/Topshelf.sln'
end

def copyOutputFiles(fromDir, filePattern, outDir)
	FileUtils.mkdir_p outDir unless exists?(outDir)
	Dir.glob(File.join(fromDir, filePattern)){|file|
		copy(file, outDir) if File.file?(file)
	}
end

desc "Runs unit tests"
nunit :tests35 => [:build35] do |nunit|
          nunit.command = File.join('src', 'packages','NUnit.Runners.2.6.1', 'tools', 'nunit-console.exe')
          nunit.options = "/framework=#{CLR_TOOLS_VERSION}", '/nothread', '/nologo', '/labels', "\"/xml=#{File.join(props[:artifacts], 'nunit-test-results-net-3.5.xml')}\""
          nunit.assemblies = FileList[File.join(props[:src], "Topshelf.Tests/bin/Release", "Topshelf.Tests.dll")]
end

desc "Runs unit tests"
nunit :tests4 => [:build4] do |nunit|
          nunit.command = File.join('src', 'packages','NUnit.Runners.2.6.1', 'tools', 'nunit-console.exe')
          nunit.options = "/framework=#{CLR_TOOLS_VERSION}", '/nothread', '/nologo', '/labels', "\"/xml=#{File.join(props[:artifacts], 'nunit-test-results-net-4.0.xml')}\""
          nunit.assemblies = FileList[File.join(props[:src], "Topshelf.Tests/bin/Release", "Topshelf.Tests.dll")]
end

task :package => [:nuget, :zip_output]

desc "ZIPs up the build results."
zip :zip_output => [:versioning] do |zip|
	zip.directories_to_zip = [props[:output]]
	zip.output_file = "Topshelf-#{NUGET_VERSION}.zip"
	zip.output_path = props[:artifacts]
end

desc "Restore NuGet Packages"
task :nuget_restore do
  sh "#{File.join(props[:lib], 'nuget.exe')} install #{File.join(props[:src],"Topshelf.Tests","packages.config")} -Source https://nuget.org/api/v2/ -o #{File.join(props[:src],"packages")}"
  sh "#{File.join(props[:lib], 'nuget.exe')} install #{File.join(props[:src],"Topshelf.Log4Net","packages.config")} -Source https://nuget.org/api/v2/ -o #{File.join(props[:src],"packages")}"
  sh "#{File.join(props[:lib], 'nuget.exe')} install #{File.join(props[:src],"Topshelf.NLog","packages.config")} -Source https://nuget.org/api/v2/ -o #{File.join(props[:src],"packages")}"
end

desc "Builds the nuget package"
task :nuget => [:versioning, :create_nuspec] do
  sh "#{File.join(props[:lib], 'nuget.exe')} pack #{props[:artifacts]}/Topshelf.nuspec /Symbols /OutputDirectory #{props[:artifacts]}"
  sh "#{File.join(props[:lib], 'nuget.exe')} pack #{props[:artifacts]}/Topshelf.Log4Net.nuspec /Symbols /OutputDirectory #{props[:artifacts]}"
  sh "#{File.join(props[:lib], 'nuget.exe')} pack #{props[:artifacts]}/Topshelf.NLog.nuspec /Symbols /OutputDirectory #{props[:artifacts]}"
  sh "#{File.join(props[:lib], 'nuget.exe')} pack #{props[:artifacts]}/Topshelf.Rehab.nuspec /Symbols /OutputDirectory #{props[:artifacts]}"
	sh "#{File.join(props[:lib], 'nuget.exe')} pack #{props[:artifacts]}/Topshelf.Supervise.nuspec /Symbols /OutputDirectory #{props[:artifacts]}"
end

nuspec :create_nuspec do |nuspec|
  nuspec.id = 'Topshelf'
  nuspec.version = NUGET_VERSION
  nuspec.authors = 'Chris Patterson, Dru Sellers, Travis Smith'
  nuspec.summary = 'Topshelf, Friction-free Windows Services'
  nuspec.description = 'Topshelf is an open source project for hosting services without friction. By referencing Topshelf, your console application *becomes* a service installer with a comprehensive set of command-line options for installing, configuring, and running your application as a service.'
  nuspec.title = 'Topshelf'
  nuspec.projectUrl = 'http://github.com/Topshelf/Topshelf'
  nuspec.iconUrl = 'http://topshelf-project.com/wp-content/themes/pandora/slide.1.png'
  nuspec.language = "en-US"
  nuspec.licenseUrl = "http://www.apache.org/licenses/LICENSE-2.0"
  nuspec.requireLicenseAcceptance = "false"
  nuspec.output_file = File.join(props[:artifacts], 'Topshelf.nuspec')
  add_files props[:output], 'Topshelf.{dll,pdb,xml}', nuspec
  nuspec.file(File.join(props[:src], "Topshelf\\**\\*.cs").gsub("/","\\"), "src")
end

nuspec :create_nuspec do |nuspec|
  nuspec.id = 'Topshelf.Log4Net'
  nuspec.version = NUGET_VERSION
  nuspec.authors = 'Chris Patterson, Dru Sellers, Travis Smith'
  nuspec.summary = 'Topshelf, Friction-free Windows Services'
  nuspec.description = 'Log4Net Logging Integration for Topshelf. Topshelf is an open source project for hosting services without friction. By referencing Topshelf, your console application *becomes* a service installer with a comprehensive set of command-line options for installing, configuring, and running your application as a service.'
  nuspec.title = 'Topshelf.Log4Net'
  nuspec.projectUrl = 'http://github.com/Topshelf/Topshelf'
  nuspec.iconUrl = 'http://topshelf-project.com/wp-content/themes/pandora/slide.1.png'
  nuspec.language = "en-US"
  nuspec.licenseUrl = "http://www.apache.org/licenses/LICENSE-2.0"
  nuspec.requireLicenseAcceptance = "false"
  nuspec.dependency "Topshelf", NUGET_VERSION
  nuspec.dependency "Log4Net", "2.0.0"
  nuspec.output_file = File.join(props[:artifacts], 'Topshelf.Log4Net.nuspec')
  add_files props[:output], 'Topshelf.Log4Net.{dll,pdb,xml}', nuspec
  nuspec.file(File.join(props[:src], "Topshelf.Log4Net\\**\\*.cs").gsub("/","\\"), "src")
end

nuspec :create_nuspec do |nuspec|
  nuspec.id = 'Topshelf.NLog'
  nuspec.version = NUGET_VERSION
  nuspec.authors = 'Chris Patterson, Dru Sellers, Travis Smith'
  nuspec.summary = 'Topshelf, Friction-free Windows Services'
  nuspec.description = 'NLog Logging Integration for Topshelf. Topshelf is an open source project for hosting services without friction. By referencing Topshelf, your console application *becomes* a service installer with a comprehensive set of command-line options for installing, configuring, and running your application as a service.'
  nuspec.title = 'Topshelf.NLog'
  nuspec.projectUrl = 'http://github.com/Topshelf/Topshelf'
  nuspec.iconUrl = 'http://topshelf-project.com/wp-content/themes/pandora/slide.1.png'
  nuspec.language = "en-US"
  nuspec.licenseUrl = "http://www.apache.org/licenses/LICENSE-2.0"
  nuspec.requireLicenseAcceptance = "false"
  nuspec.dependency "Topshelf", NUGET_VERSION
  nuspec.dependency "NLog", "2.0"
  nuspec.output_file = File.join(props[:artifacts], 'Topshelf.NLog.nuspec')
  add_files props[:output], 'Topshelf.NLog.{dll,pdb,xml}', nuspec
  nuspec.file(File.join(props[:src], "Topshelf.NLog\\**\\*.cs").gsub("/","\\"), "src")
end

nuspec :create_nuspec do |nuspec|
  nuspec.id = 'Topshelf.Rehab'
  nuspec.version = NUGET_VERSION
  nuspec.authors = 'Chris Patterson, Dru Sellers, Travis Smith'
  nuspec.summary = 'Topshelf, Friction-free Windows Services'
  nuspec.description = 'Rehab provides automatic updates to services. Topshelf is an open source project for hosting services without friction. By referencing Topshelf, your console application *becomes* a service installer with a comprehensive set of command-line options for installing, configuring, and running your application as a service.'
  nuspec.title = 'Topshelf.Rehab'
  nuspec.projectUrl = 'http://github.com/Topshelf/Topshelf'
  nuspec.iconUrl = 'http://topshelf-project.com/wp-content/themes/pandora/slide.1.png'
  nuspec.language = "en-US"
  nuspec.licenseUrl = "http://www.apache.org/licenses/LICENSE-2.0"
  nuspec.requireLicenseAcceptance = "false"
  nuspec.dependency "Topshelf", NUGET_VERSION
  nuspec.output_file = File.join(props[:artifacts], 'Topshelf.Rehab.nuspec')
  add_files props[:output], 'Topshelf.Rehab.{dll,pdb,xml}', nuspec
  nuspec.file(File.join(props[:src], "Topshelf.Rehab\\**\\*.cs").gsub("/","\\"), "src")
end

nuspec :create_nuspec do |nuspec|
  nuspec.id = 'Topshelf.Supervise'
  nuspec.version = NUGET_VERSION
  nuspec.authors = 'Chris Patterson, Dru Sellers, Travis Smith'
  nuspec.summary = 'Topshelf, Supervised Services'
  nuspec.description = 'Supervise provides automatic recovery, memory and CPU monitoring, and scheduled restarting to services. Topshelf is an open source project for hosting services without friction. By referencing Topshelf, your console application *becomes* a service installer with a comprehensive set of command-line options for installing, configuring, and running your application as a service.'
  nuspec.title = 'Topshelf.Supervise'
  nuspec.projectUrl = 'http://github.com/Topshelf/Topshelf'
  nuspec.iconUrl = 'http://topshelf-project.com/wp-content/themes/pandora/slide.1.png'
  nuspec.language = "en-US"
  nuspec.licenseUrl = "http://www.apache.org/licenses/LICENSE-2.0"
  nuspec.requireLicenseAcceptance = "false"
  nuspec.dependency "Topshelf", NUGET_VERSION
  nuspec.output_file = File.join(props[:artifacts], 'Topshelf.Supervise.nuspec')
  add_files props[:output], 'Topshelf.Supervise.{dll,pdb,xml}', nuspec
  nuspec.file(File.join(props[:src], "Topshelf.Supervise\\**\\*.cs").gsub("/","\\"), "src")
end

def project_outputs(props)
	props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.dll" }.
		concat( props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.exe" } ).
		find_all{ |path| exists?(path) }
end

def get_commit_hash_and_date
	begin
		commit = `git log -1 --pretty=format:%H`
		git_date = `git log -1 --date=iso --pretty=format:%ad`
		commit_date = DateTime.parse( git_date ).strftime("%Y-%m-%d %H%M%S")
	rescue
		commit = "git unavailable"
	end

	[commit, commit_date]
end

def add_files stage, what_dlls, nuspec
  [['net35', 'net-3.5'], ['net40', 'net-4.0'], ['net40-full', 'net-4.0-full']].each{|fw|
    takeFrom = File.join(stage, fw[1], what_dlls)
    Dir.glob(takeFrom).each do |f|
      nuspec.file(f.gsub("/", "\\"), "lib\\#{fw[0]}")
    end
  }
end

def waitfor(&block)
	checks = 0

	until block.call || checks >10
		sleep 0.5
		checks += 1
	end

	raise 'Waitfor timeout expired. Make sure that you aren\'t running something from the build output folders, or that you have browsed to it through Explorer.' if checks > 10
end
