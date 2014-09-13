require 'bundler/setup'

require 'albacore'
require 'albacore/tasks/versionizer'
require 'albacore/ext/teamcity'

Albacore::Tasks::Versionizer.new :versioning

desc 'restore all nugets as per the packages.config files'
nugets_restore :restore do |p|
  p.out = 'src/packages'
  p.exe = 'tools/NuGet.exe'
end

asmver :asmver => :versioning do |a|
  a.file_path  = 'src/SolutionVersion.cs'
  a.namespace  = 'Topshelf'
  a.attributes assembly_title: 'Topshelf',
               assembly_version: ENV['LONG_VERSION'],
               assembly_file_version: ENV['LONG_VERSION'],
               assembly_informational_version: ENV['BUILD_VERSION'],
               assembly_copyright: "Copyright #{Time.now.year} Chris Patterson, Dru Sellers, Travis Smith, All rights reserved.",
               assembly_description: 'Topshelf is an open source project for hosting services without friction. By referencing Topshelf, your console application *becomes* a service installer with a comprehensive set of command-line options for installing, configuring, and running your application as a service.',
               com_visible: false
end

desc 'Perform fast build (warn: doesn\'t d/l deps)'
build :quick_build do |b|
  b.logging = 'detailed'
  b.sln     = 'src/MyProj.sln'
end

desc 'Perform full build'
build :build => [:versioning, :restore] do |b|
  b.sln = 'src/MyProj.sln'
  # alt: b.file = 'src/MyProj.sln'
end

directory 'build/pkg'

desc 'package nugets - finds all projects and package them'
nugets_pack :create_nugets => ['build/pkg', :versioning, :build] do |p|
  p.files   = FileList['src/**/*.{csproj,fsproj,nuspec}'].
    exclude(/Tests/)
  p.out     = 'build/pkg'
  p.exe     = 'buildsupport/NuGet.exe'
  p.with_metadata do |m|
    m.description = 'A cool nuget'
    m.authors = 'Henrik'
    m.version = ENV['NUGET_VERSION']
  end
  p.with_package do |p|
    p.add_file 'file/relative/to/proj', 'lib/net40'
  end
end

desc 'Create nugets and zip outputs for distribution'
task :package => [:create_nugets, :zip_output]

desc '**Default**, compiles and runs tests'
task :default => [:clean, :nuget_restore, :compile, :package]
