require 'albacore/albacoretask'
require 'build_support/ilmergeconfig'

class ILMerge
  include Albacore::Task
  include Albacore::RunCommand
  include Configuration::ILMerge
  
  attr_accessor :assembly, :output, :debug, :target, :allow_dupes, :log, :internalize
  attr_array :references

  def initialize
	@debug = false
	@allow_dupes = true
	@platform_version = "v4"
	@platform_directory = get_net_version(:net4)
	
    super()
    update_attributes ilmerge.to_hash
  end

  def execute
    params = []
    params << "/out:#{@output}" unless @output.nil?
	params << "/log:#{@log}" unless @log.nil?
	params << "/internalize:#{@internalize}" unless @internalize.nil?
    params << "/target:#{@target}" unless @target.nil?
    params << "/allowDup" unless @allow_dupes.nil?
    params << "/ndebug" unless @debug
	params << "/targetplatform:#{@platform_version},#{@platform_directory}" unless @platform_version.nil?
	params << "#{@assembly}"
    params << @references.map{|r| format_reference(r)} unless @references.nil?
    
	puts "Running ILMerge: " + params.join(' ')
    result = run_command "ILMerge", params
    
    failure_message = 'ILMerge Failed. See Build Log For Detail'
    fail_with_message failure_message if !result
  end
	#, "/internalize:build.custom/ilmerge.internalize.ignore.txt /target:dll /out:code_drop/#{OUTPUT_PATH}/Topshelf.dll /log:code_drop/ilmerge.log /ndebug /allowDup Topshelf.dll Magnum.dll Newtonsoft.Json.dll Spark.dll Stact.dll Stact.ServerFramework.dll"
	
  def platform_directory(netversion)
	get_net_version(netversion)
  end
	
  def format_reference(resource)
    "#{resource}"
  end
end
