# encoding: utf-8
module TypeCheckUtil
  def check_type(value, type)
    TypeCheckUtil::check_type(value, type)
  end

  def self.check_type(value, type)
    found = false
    if type.is_a? Array
      type.each do |t|
        if value.is_a? t
          found = true
          break
        end
      end
    else
      found = value.is_a? type
    end

    raise TypeError, "Expected #{type}, but got #{value.class}" unless found
  end

  def check_arguments(args, types)
    TypeCheckUtil::check_arguments(args, types)
  end

  def self.check_arguments(args, types)
    raise ArgumentError, "Invalid number of arguments" unless args.size == types.size

    begin
      idx = 0
      args.each_with_index do |arg, i|
        idx = i
        check_type(arg, types[i])
      end
    rescue TypeError => e
      raise ArgumentError, "Invalid argument at index #{idx}: #{e.message}"
    end
  end
end