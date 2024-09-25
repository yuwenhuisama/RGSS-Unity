require 'type_check_util'

class Tone
  include TypeCheckUtil

  attr_reader :__handler__

  def initialize(*args)
    if args.size == 0
      @__handler__ = Unity::Tone.new_rgbg(0, 0, 0, 0)
    elsif args.size == 1
      check_arguments(args, [Unity::Tone])
      tone, = args
      @__handler__, = tone
    elsif args.size == 3
      check_arguments(args, [[Integer, Float], [Integer, Float], [Integer, Float]])
      r, g, b = args
      @__handler__ = Unity::Tone.new_rgbg r, g, b, 0
    elsif args.size == 4
      check_arguments(args, [[Integer, Float], [Integer, Float], [Integer, Float], [Integer, Float]])
      r, g, b, gray = args
      @__handler__ = Unity::Tone.new_rgbg r, g, b, gray
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def set(*args)
    if args.size == 1
      check_arguments(args, [Tone])
      tone, = args
      self.set(tone.red, tone.green, tone.blue, tone.gray)
    elsif args.size == 4
      check_arguments(args, [[Integer, Float], [Integer, Float], [Integer, Float], [Integer, Float]])

      r, g, b, gray = args
      @__handler__.set_rgbg(r, g, b, gray)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def eql?(other)
    unless other.is_a? Tone
      return false
    end
    
    if self == other || self.__handler__ == other.__handler__
      true
    end
    
    self.red == other.red && self.green == other.green && self.blue == other.blue && self.gray == other.gray
  end

  def hash
    @__handler__.hash
  end

  def _dump(d = 0)
    [self.red, self.green, self.blue, self.gray].pack('d4')
  end

  def self._load(s)
    Tone.new(*s.unpack('d4'))
  end

  [:red, :green, :blue, :gray].each do |prop|
    define_method(prop) { @__handler__.send(prop) }
    define_method("#{prop}=") do |value|
      check_type(value, [Integer, Float])
      @__handler__.send("#{prop}=", value)
    end
  end
end