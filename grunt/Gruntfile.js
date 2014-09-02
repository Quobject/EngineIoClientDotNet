
module.exports = function (grunt) {
  var
    node_os = require('os'),
    config = require('./config.json'),
    util = require('util'),
    os = node_os.platform() === 'win32' ? 'win' : 'linux';

  grunt.log.writeln(util.inspect(config));
  grunt.log.writeln( 'os = "%s"', os );

  grunt.loadTasks('./tasks');

  grunt.initConfig({      
    os: os,
    config: config,
    msbuild_configuration: 'Debug',
    //msbuild_configuration: 'Release',
    release_path:  './../Releases/<%= config.version %>/', 
    server_path: '../TestServer/',
    shell: {
      exec: {
        options: {
          stdout: true,
          stderr: true
        }
      }
    },
    jshint: {
      options: {
        jshintrc: true,
      },
      target: ['Gruntfile.js', '<%= server_path %>server.js', 'tasks/**/*.js']
    },
    clean: {
      release : [ '<%= release_path %>/*' ],
      options: { force: true }                
    },  
    copy: {
      release: {
        files: [
          {
            expand: true,
            cwd:  './../EngineIoClientDotNet/bin/Release',
            src:  '*',
            dest: '<%= release_path %>/net45'
          }
        ]
      },
      release_mono: {
        files: [
          {
            expand: true,
            cwd:  './../EngineIoClientDotNet_Mono/bin/Release',
            src:  '*',
            dest: '<%= release_path %>/mono'
          }
        ]
      },
    }
  });

  grunt.loadNpmTasks('grunt-contrib-copy');
  grunt.loadNpmTasks('grunt-contrib-clean');  
  grunt.loadNpmTasks('grunt-shell');
  grunt.loadNpmTasks('grunt-contrib-jshint');
  grunt.registerTask('default', ['jshint', 'installNpm', 'nuget', 'buildClient', 'buildTest', 'startServer', 'testClient']);
  grunt.registerTask('test', ['jshint', 'buildClient', 'buildTest', 'testClient']);
};
