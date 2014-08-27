module.exports = function (grunt) {

  grunt.registerTask('nuget',
      'get assemplys ', function () {
    var
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      tasks = [],
      format_str = os === 'win' ?
        '{0} restore {1}' :
        '',
      nuget_path = os === 'win' ?
        config.win.nuget : config.linux.nuget,    
      solution = __dirname + '/../../EngineIoClientDotNet.sln';


    grunt.log.writeln('nuget_path = "%s"', nuget_path);

    tasks.push(  string.format(format_str,nuget_path, solution) );

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');

  });
};


