require 'type_check_util'

class Tone
  include TypeCheckUtil

  attr_reader :handler

  def initialize(*args)
    if args.size == 1
      check_arguments(args, [Unity::Tone])
      tone, = args
      @handler, = Unity::Tone.new_rgbg t.red, t.green, t.blue, t.gray
    elsif args.size == 3
      check_arguments(args, [Integer, Integer, Integer])
      r, g, b = args
      @handler = Unity::Tone.new_rgbg r, g, b, 0
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])
      r, g, b, gray = args
      @handler = Unity::Tone.new_rgbg r, g, b, gray
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
      check_arguments(args, [Integer, Integer, Integer, Integer])

      r, g, b, gray = args
      @handler.set_rgbg(r, g, b, gray)
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  [:red, :green, :blue, :gray].each do |prop|
    define_method(prop) { @handler.send(prop) }
    define_method("#{prop}=") do |value|
      check_type(value, Integer)
      @handler.send("#{prop}=", value)
    end
  end
end